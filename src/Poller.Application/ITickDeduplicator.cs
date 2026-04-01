using Poller.Model;

namespace Poller.Application;

public interface ITickDeduplicator
{
    bool IsDuplicate(NormalizedTick tick);
}
