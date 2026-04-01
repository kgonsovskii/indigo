namespace Poller.Application.Abstractions;

public interface ITickMetrics
{
    void AddPersisted(long count);

    long PersistedTotal { get; }
}
