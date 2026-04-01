using Poller.Domain;

namespace Poller.Application;

public sealed record TickToPersist(NormalizedTick Tick, string? RawPayload);
