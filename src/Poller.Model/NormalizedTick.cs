namespace Poller.Model;

public sealed record NormalizedTick(string ExchangeId, string Symbol, decimal Price, decimal Volume, DateTimeOffset TimestampUtc);
