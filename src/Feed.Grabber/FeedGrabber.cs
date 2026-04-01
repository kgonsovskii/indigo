using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Channels;
using Feed.Parser.Base;
using Microsoft.Extensions.Options;
using Poller.Application;
using Poller.Model;

namespace Feed.Grabber;

public sealed class FeedGrabber<TParser>
    where TParser : class, IFeedParser
{
    private readonly TParser _parser;
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly IOptionsMonitor<FeedGrabberOptions> _grabberOptions;
    private readonly string _grabberOptionsName;
    private readonly ChannelWriter<TickToPersist> _writer;
    private readonly ITickDeduplicator _deduplicator;
    private readonly IOptions<IngestionOptions> _ingestion;
    private readonly IFeedConnectionTelemetry _feedTelemetry;

    public FeedGrabber(
        TParser parser,
        IOptionsMonitor<FeedGrabberOptions> grabberOptions,
        ChannelWriter<TickToPersist> writer,
        ITickDeduplicator deduplicator,
        IOptions<IngestionOptions> ingestion,
        IFeedConnectionTelemetry feedTelemetry)
    {
        _parser = parser;
        _grabberOptions = grabberOptions;
        _grabberOptionsName = TParser.ConfigurationSectionKey;
        _writer = writer;
        _deduplicator = deduplicator;
        _ingestion = ingestion;
        _feedTelemetry = feedTelemetry;
    }

    private FeedGrabberOptions GrabberOptions => _grabberOptions.Get(_grabberOptionsName);

    public async Task RunAsync(int laneIndex, CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var reconnectDelay = _ingestion.Value.ReconnectDelay;
            var options = GrabberOptions;
            var webSocketUri = new Uri(options.WebSocketUri);
            using var ws = new ClientWebSocket();
            try
            {
                await ws.ConnectAsync(webSocketUri, stoppingToken);
                _feedTelemetry.RecordConnected(
                    options.Name,
                    laneIndex,
                    typeof(TParser).Name,
                    webSocketUri);
                try
                {
                    await ReceiveLoopAsync(ws, options.Name, stoppingToken);
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

                    _feedTelemetry.RecordDisconnected(options.Name, laneIndex);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _feedTelemetry.RecordConnectionError(GrabberOptions.Name, laneIndex, ex, reconnectDelay);
            }

            try
            {
                await Task.Delay(reconnectDelay, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
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
