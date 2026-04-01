using Indigo.Domain;

namespace Indigo.Application;

public sealed record TickToPersist(NormalizedTick Tick, string? RawPayload);
