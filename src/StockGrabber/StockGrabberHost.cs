using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Poller.Application.Configuration;
using StockParser.Base;

namespace StockGrabber;

public sealed class StockGrabberHost<TParser> : BackgroundService
    where TParser : class, IStockParser
{
    private readonly StockGrabber<TParser> _grabber;
    private readonly IOptionsMonitor<StockGrabberOptions> _grabberOptions;
    private readonly string _grabberOptionsName;
    private readonly IOptions<IngestionOptions> _ingestion;
    private readonly ILogger<StockGrabberHost<TParser>> _logger;

    public StockGrabberHost(
        StockGrabber<TParser> grabber,
        IOptionsMonitor<StockGrabberOptions> grabberOptions,
        IOptions<IngestionOptions> ingestion,
        ILogger<StockGrabberHost<TParser>> logger)
    {
        _grabber = grabber;
        _grabberOptions = grabberOptions;
        _grabberOptionsName = TParser.ConfigurationSectionKey;
        _ingestion = ingestion;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var opt = _grabberOptions.Get(_grabberOptionsName);
        var degree = Math.Max(1, opt.DegreeOfParallelism);
        var feedName = opt.Name;
        _logger.LogInformation("Grabber host starting {Feed} lanes {Lanes}", feedName, degree);
        var tasks = new Task[degree];
        for (var lane = 0; lane < degree; lane++)
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
                _logger.LogError(ex, "Grabber lane stopped {Feed} lane {Lane}, restarting after delay", feedName, laneIndex);
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
