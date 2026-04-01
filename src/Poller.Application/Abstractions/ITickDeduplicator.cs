using Poller.Domain;

namespace Poller.Application.Abstractions;

public interface ITickDeduplicator
{
    bool IsDuplicate(NormalizedTick tick);
}
