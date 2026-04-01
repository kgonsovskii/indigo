namespace Poller.Model;

public sealed class TickRecord
{
    private TickRecord()
    {
    }

    public static TickRecord Create(NormalizedTick tick, string? rawPayload, DateTimeOffset ingestedAtUtc) =>
        new()
        {
            ExchangeId = tick.ExchangeId,
            Symbol = tick.Symbol,
            Price = tick.Price,
            Volume = tick.Volume,
            TimestampUtc = tick.TimestampUtc,
            RawPayload = rawPayload,
            IngestedAtUtc = ingestedAtUtc,
        };

    public long Id { get; init; }

    public string ExchangeId { get; init; } = string.Empty;

    public string Symbol { get; init; } = string.Empty;

    public decimal Price { get; init; }

    public decimal Volume { get; init; }

    public DateTimeOffset TimestampUtc { get; init; }

    public DateTimeOffset IngestedAtUtc { get; init; }

    public string? RawPayload { get; init; }
}
