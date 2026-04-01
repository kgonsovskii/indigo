namespace Poller.Model;

public sealed class TickRecord
{
    public long Id { get; set; }

    public string ExchangeId { get; set; } = string.Empty;

    public string Symbol { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public decimal Volume { get; set; }

    public DateTimeOffset TimestampUtc { get; set; }

    public DateTimeOffset IngestedAtUtc { get; set; }

    public string? RawPayload { get; set; }
}
