using System.Diagnostics;
using Feed.Parser.Base;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Poller.Application;

namespace Feed.Grabber;

public sealed class FeedGrabberHost<TParser> : BackgroundService
    where TParser : class, IFeedParser
{
    private readonly FeedGrabber<TParser> _grabber;
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly IOptionsMonitor<FeedGrabberOptions> _grabberOptions;
    private readonly string _grabberOptionsName;
    private readonly IOptions<IngestionOptions> _ingestion;
    private readonly ILogger<FeedGrabberHost<TParser>> _logger;

    public FeedGrabberHost(
        FeedGrabber<TParser> grabber,
        IOptionsMonitor<FeedGrabberOptions> grabberOptions,
        IOptions<IngestionOptions> ingestion,
        ILogger<FeedGrabberHost<TParser>> logger)
    {
        _grabber = grabber;
        _grabberOptions = grabberOptions;
        _grabberOptionsName = TParser.ConfigurationSectionKey;
        _ingestion = ingestion;
        _logger = logger;
    }

    private FeedGrabberOptions GrabberOptions => _grabberOptions.Get(_grabberOptionsName);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var opt = GrabberOptions;
        var degreeOfParallelism = Math.Max(1, opt.DegreeOfParallelism);
        var feedName = opt.Name;
        var feedUrl = opt.WebSocketUri;
        _logger.LogInformation(
            "Grabber host starting {Feed} {FeedUrl} {DegreeOfParallelism}",
            feedName,
            feedUrl,
            degreeOfParallelism);
        var tasks = new Task[degreeOfParallelism];
        for (var lane = 0; lane < degreeOfParallelism; lane++)
        {
            var laneIndex = lane;
            tasks[lane] = RunLaneAsync(laneIndex, feedName, stoppingToken);
        }

        await Task.WhenAll(tasks);
    }

    private async Task RunLaneAsync(int laneIndex, string feedName, CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _grabber.RunAsync(stoppingToken);
                return;
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    "Grabber lane stopped {Feed} lane {Lane}, restarting after delay: {Message}",
                    feedName,
                    laneIndex,
                    ex.Message);
                var delayMs = Math.Max(1, _ingestion.Value.ReconnectInitialDelayMs);
                try
                {
                    await Task.Delay(delayMs, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    return;
                }
            }
        }
    }
}
