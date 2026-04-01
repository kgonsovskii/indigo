namespace Poller.Model;

public sealed class TickRecord
{
    public TickRecord(NormalizedTick tick, string? rawPayload)
    {
        ExchangeId = tick.ExchangeId;
        Symbol = tick.Symbol;
        Price = tick.Price;
        Volume = tick.Volume;
        TimestampUtc = tick.TimestampUtc;
        RawPayload = rawPayload;
    }

    public long Id { get; init; }

    public string ExchangeId { get; init; }

    public string Symbol { get; init; }

    public decimal Price { get; init; }

    public decimal Volume { get; init; }

    public DateTimeOffset TimestampUtc { get; init; }

    public DateTimeOffset IngestedAtUtc { get; init; }

    public string? RawPayload { get; init; }
}
