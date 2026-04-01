namespace Poller.Application;

public interface ITickMetrics
{
    void AddPersisted(long count, IReadOnlyDictionary<string, int>? perExchange = null);

    long PersistedTotal { get; }

    double RecentPersistRatePerSecond { get; }

    IReadOnlyDictionary<string, long> PersistedTotalByExchange { get; }
}
