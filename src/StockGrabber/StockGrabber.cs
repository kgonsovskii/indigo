using System.Net.WebSockets;
using System.Text;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Poller.Application;
using Poller.Application.Abstractions;
using Poller.Application.Configuration;
using StockParser.Base;

namespace StockGrabber;

public interface IStockGrabber
{
    Task RunAsync(CancellationToken cancellationToken);
}

public sealed class StockGrabber<TParser> : IStockGrabber
    where TParser : class, IStockParser
{
    private readonly TParser _parser;
    private readonly IOptionsMonitor<StockGrabberOptions> _grabberOptions;
    private readonly string _grabberOptionsName;
    private readonly ChannelWriter<TickToPersist> _writer;
    private readonly ITickDeduplicator _deduplicator;
    private readonly IOptions<IngestionOptions> _ingestion;
    private readonly ILogger<StockGrabber<TParser>> _logger;

    public StockGrabber(
        TParser parser,
        IOptionsMonitor<StockGrabberOptions> grabberOptions,
        ChannelWriter<TickToPersist> writer,
        ITickDeduplicator deduplicator,
        IOptions<IngestionOptions> ingestion,
        ILogger<StockGrabber<TParser>> logger)
    {
        _parser = parser;
        _grabberOptions = grabberOptions;
        _grabberOptionsName = TParser.ConfigurationSectionKey;
        _writer = writer;
        _deduplicator = deduplicator;
        _ingestion = ingestion;
        _logger = logger;
    }

    private StockGrabberOptions GrabberOptions => _grabberOptions.Get(_grabberOptionsName);

    public async Task RunAsync(CancellationToken stoppingToken)
    {
        var delayMs = Math.Max(1, _ingestion.Value.ReconnectInitialDelayMs);
        var maxDelay = Math.Max(delayMs, _ingestion.Value.ReconnectMaxDelayMs);
        while (!stoppingToken.IsCancellationRequested)
        {
            var options = GrabberOptions;
            var webSocketUri = new Uri(options.WebSocketUri);
            using var ws = new ClientWebSocket();
            try
            {
                await ws.ConnectAsync(webSocketUri, stoppingToken);
                _logger.LogInformation("Grabber connected {Name} {Parser} {Uri}", options.Name, typeof(TParser).Name, webSocketUri);
                delayMs = Math.Max(1, _ingestion.Value.ReconnectInitialDelayMs);
                await ReceiveLoopAsync(ws, options.Name, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Grabber error {Name} reconnect in {DelayMs}ms", GrabberOptions.Name, delayMs);
            }
            finally
            {
                if (ws.State == WebSocketState.Open)
                {
                    try
                    {
                        await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                    }
                    catch
                    {
                        //nothing
                    }
                }
            }

            _logger.LogInformation("Grabber disconnected {Name}", GrabberOptions.Name);
            try
            {
                await Task.Delay(delayMs, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }

            delayMs = Math.Min(delayMs * 2, maxDelay);
        }
    }

    private async Task ReceiveLoopAsync(ClientWebSocket ws, string feedName, CancellationToken ct)
    {
        var buffer = new byte[65536];
        await using var ms = new MemoryStream(8192);
        while (ws.State == WebSocketState.Open && !ct.IsCancellationRequested)
        {
            var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), ct);
            if (result.MessageType == WebSocketMessageType.Close)
            {
                break;
            }

            if (result.MessageType != WebSocketMessageType.Text)
            {
                if (result.EndOfMessage)
                {
                    ms.SetLength(0);
                }

                continue;
            }

            ms.Write(buffer.AsSpan(0, result.Count));
            if (!result.EndOfMessage)
            {
                continue;
            }

            var text = Encoding.UTF8.GetString(ms.GetBuffer(), 0, (int)ms.Length);
            ms.SetLength(0);
            if (!_parser.TryParse(text, feedName, out var tick) || tick is null)
            {
                continue;
            }

            if (_deduplicator.IsDuplicate(tick))
            {
                continue;
            }

            await _writer.WriteAsync(new TickToPersist(tick, text), ct);
        }
    }
}
