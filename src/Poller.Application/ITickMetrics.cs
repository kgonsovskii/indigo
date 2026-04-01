namespace Poller.Application;

public interface ITickMetrics
{
    void AddPersisted(long count);

    long PersistedTotal { get; }
}
