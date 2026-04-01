using Poller.Application.Abstractions;

namespace Poller.Infrastructure.Processing;

public sealed class TickMetrics : ITickMetrics
{
    private long _persisted;

    public void AddPersisted(long count)
    {
        Interlocked.Add(ref _persisted, count);
    }

    public long PersistedTotal => Interlocked.Read(ref _persisted);
}
