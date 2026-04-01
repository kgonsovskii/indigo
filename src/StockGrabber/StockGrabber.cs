using System.Net.WebSockets;
using System.Text;
using System.Threading.Channels;
using Indigo.Application;
using Indigo.Application.Abstractions;
using Indigo.Application.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StockParser.Base;

namespace StockGrabber;

public sealed class StockGrabber<TParser> : BackgroundService
    where TParser : class, IStockParser
{
    private readonly TParser _parser;
    private readonly GrabberEndpointBinding _binding;
    private readonly ChannelWriter<TickToPersist> _writer;
    private readonly ITickDeduplicator _deduplicator;
    private readonly IOptions<IngestionOptions> _ingestion;
    private readonly ILogger<StockGrabber<TParser>> _logger;

    public StockGrabber(
        TParser parser,
        GrabberEndpointBinding binding,
        ChannelWriter<TickToPersist> writer,
        ITickDeduplicator deduplicator,
        IOptions<IngestionOptions> ingestion,
        ILogger<StockGrabber<TParser>> logger)
    {
        _parser = parser;
        _binding = binding;
        _writer = writer;
        _deduplicator = deduplicator;
        _ingestion = ingestion;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var delayMs = Math.Max(1, _ingestion.Value.ReconnectInitialDelayMs);
        var maxDelay = Math.Max(delayMs, _ingestion.Value.ReconnectMaxDelayMs);
        while (!stoppingToken.IsCancellationRequested)
        {
            using var ws = new ClientWebSocket();
            try
            {
                await ws.ConnectAsync(_binding.WebSocketUri, stoppingToken);
                _logger.LogInformation("Grabber connected {Name} {Parser} {Uri}", _binding.Name, typeof(TParser).Name, _binding.WebSocketUri);
                delayMs = Math.Max(1, _ingestion.Value.ReconnectInitialDelayMs);
                await ReceiveLoopAsync(ws, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Grabber error {Name} reconnect in {DelayMs}ms", _binding.Name, delayMs);
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
                    }
                }
            }

            _logger.LogInformation("Grabber disconnected {Name}", _binding.Name);
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

    private async Task ReceiveLoopAsync(ClientWebSocket ws, CancellationToken ct)
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
            if (!_parser.TryParse(text, _binding.Name, out var tick) || tick is null)
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
