using Microsoft.EntityFrameworkCore;
using Poller.Application;
using Poller.Model;

namespace Poller.Storage;

public sealed class EfTickPersistence : ITickPersistence
{
    private readonly IDbContextFactory<TickDbContext> _factory;

    public EfTickPersistence(IDbContextFactory<TickDbContext> factory)
    {
        _factory = factory;
    }

    public async Task SaveBatchAsync(IReadOnlyList<TickToPersist> ticks, CancellationToken cancellationToken)
    {
        if (ticks.Count == 0)
        {
            return;
        }

        await using var db = await _factory.CreateDbContextAsync(cancellationToken);
        var now = DateTimeOffset.UtcNow;
        foreach (var item in ticks)
        {
            db.Ticks.Add(TickRecord.Create(item.Tick, item.RawPayload, now));
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
