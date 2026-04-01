namespace Poller.Model;

public sealed record TickToPersist(NormalizedTick Tick, string? RawPayload);
