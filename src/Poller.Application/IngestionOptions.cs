namespace Poller.Application;

public sealed class IngestionOptions
{
    public const string SectionName = "Ingestion";

    public int BatchSize { get; set; } = 100;

    public TimeSpan BatchMaxWait { get; set; } = TimeSpan.FromMilliseconds(200);

    public TimeSpan DeduplicationWindow { get; set; } = TimeSpan.FromMilliseconds(750);

    public TimeSpan ReconnectDelay { get; set; } = TimeSpan.FromSeconds(3);

    public TimeSpan MetricsRateWindow { get; set; } = TimeSpan.FromSeconds(5);

    public TimeSpan MetricsLogInterval { get; set; } = TimeSpan.FromSeconds(3);
}
