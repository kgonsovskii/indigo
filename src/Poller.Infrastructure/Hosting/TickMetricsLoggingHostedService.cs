using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Poller.Application.Abstractions;

namespace Poller.Infrastructure.Hosting;

public sealed class TickMetricsLoggingHostedService : BackgroundService
{
    private readonly ITickMetrics _metrics;
    private readonly ILogger<TickMetricsLoggingHostedService> _logger;

    public TickMetricsLoggingHostedService(ITickMetrics metrics, ILogger<TickMetricsLoggingHostedService> logger)
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
