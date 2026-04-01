using Poller.Model;

namespace Poller.Application;

public interface ITickPersistence
{
    Task SaveBatchAsync(IReadOnlyList<TickToPersist> ticks, CancellationToken cancellationToken);
}
