namespace Indigo.Application.Abstractions;

public interface ITickPersistence
{
    Task SaveBatchAsync(IReadOnlyList<TickToPersist> ticks, CancellationToken cancellationToken);
}
