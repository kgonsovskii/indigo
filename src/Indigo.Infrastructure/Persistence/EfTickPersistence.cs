using Indigo.Application;
using Indigo.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Indigo.Infrastructure.Persistence;

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
            db.Ticks.Add(new TickRecord
            {
                ExchangeId = item.Tick.ExchangeId,
                Symbol = item.Tick.Symbol,
                Price = item.Tick.Price,
                Volume = item.Tick.Volume,
                TimestampUtc = item.Tick.TimestampUtc,
                IngestedAtUtc = now,
                RawPayload = item.RawPayload,
            });
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
