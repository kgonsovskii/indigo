using Indigo.Domain;

namespace Indigo.Application.Abstractions;

public interface ITickDeduplicator
{
    bool IsDuplicate(NormalizedTick tick);
}
