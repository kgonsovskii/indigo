namespace Indigo.Application.Configuration;

public sealed class IngestionOptions
{
    public const string SectionName = "Ingestion";

    public int BatchSize { get; set; } = 100;

    public int BatchMaxWaitMs { get; set; } = 200;

    public int DeduplicationWindowMs { get; set; } = 750;

    public int ReconnectInitialDelayMs { get; set; } = 500;

    public int ReconnectMaxDelayMs { get; set; } = 30_000;
}
