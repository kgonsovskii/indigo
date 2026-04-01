using System.Text;
using Microsoft.Extensions.Options;
using Poller.Application;

namespace Poller;

public sealed class MetricsLogging : BackgroundService
{
    private readonly ITickMetrics _metrics;
    private readonly IFeedConnectionTelemetry _feedTelemetry;
    private readonly IOptions<IngestionOptions> _ingestion;
    private readonly ILogger<MetricsLogging> _logger;

    public MetricsLogging(
        ITickMetrics metrics,
        IFeedConnectionTelemetry feedTelemetry,
        IOptions<IngestionOptions> ingestion,
        ILogger<MetricsLogging> logger)
    {
        _metrics = metrics;
        _feedTelemetry = feedTelemetry;
        _ingestion = ingestion;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(_ingestion.Value.MetricsLogInterval);
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await timer.WaitForNextTickAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }

            var rate = _metrics.RecentPersistRatePerSecond;
            var total = _metrics.PersistedTotal;
            var byExchange = _metrics.PersistedTotalByExchange;
            var lanes = _feedTelemetry.GetActiveLanesSnapshot();
            var byExchangeText = FormatByExchange(byExchange);
            var lanesText = FormatLanes(lanes);

            _logger.LogInformation(
                "Poller metrics: {TicksPerSecond:F1} ticks/s (recent window), persisted total {PersistedTotal}, by exchange [{ByExchange}], active lanes [{ActiveLanes}]",
                rate,
                total,
                byExchangeText,
                lanesText);
        }
    }

    private static string FormatByExchange(IReadOnlyDictionary<string, long> byExchange)
    {
        if (byExchange.Count == 0)
        {
            return string.Empty;
        }

        var sb = new StringBuilder();
        foreach (var pair in byExchange.OrderBy(p => p.Key, StringComparer.Ordinal))
        {
            if (sb.Length > 0)
            {
                sb.Append(", ");
            }

            sb.Append(pair.Key).Append('=').Append(pair.Value);
        }

        return sb.ToString();
    }

    private static string FormatLanes(IReadOnlyDictionary<string, int> lanes)
    {
        if (lanes.Count == 0)
        {
            return "none";
        }

        var sb = new StringBuilder();
        foreach (var pair in lanes.OrderBy(p => p.Key, StringComparer.Ordinal))
        {
            if (sb.Length > 0)
            {
                sb.Append(", ");
            }

            sb.Append(pair.Key).Append('=').Append(pair.Value);
        }

        return sb.ToString();
    }
}
