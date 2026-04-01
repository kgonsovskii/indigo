using Poller.Application;

namespace Poller;

public sealed class TickMetricsLogging : BackgroundService
{
    private readonly ITickMetrics _metrics;
    private readonly ILogger<TickMetricsLogging> _logger;

    public TickMetricsLogging(ITickMetrics metrics, ILogger<TickMetricsLogging> logger)
    {
        _metrics = metrics;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            _logger.LogInformation("Processed ticks persisted total {Total}", _metrics.PersistedTotal);
        }
    }
}
