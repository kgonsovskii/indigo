using Indigo.Application;
using Indigo.Domain;
using Indigo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Indigo.Tests;

public sealed class EfTickPersistenceTests
{
    [Fact]
    public async Task SaveBatchAsync_persists_rows()
    {
        var name = $"Data Source=ticks-test-{Guid.NewGuid():N}.db";
        var options = new DbContextOptionsBuilder<TickDbContext>().UseSqlite(name).Options;
        var factory = new TestDbContextFactory(options);
        await using (var db = await factory.CreateDbContextAsync())
        {
            await db.Database.EnsureCreatedAsync();
        }

        var persistence = new EfTickPersistence(factory);
        var ts = new DateTimeOffset(2024, 6, 1, 12, 0, 0, TimeSpan.Zero);
        var items = new[]
        {
            new TickToPersist(new NormalizedTick("alpha", "BTC", 10m, 1m, ts), "{\"raw\":1}"),
            new TickToPersist(new NormalizedTick("beta", "ETH", 20m, 2m, ts), null),
        };
        await persistence.SaveBatchAsync(items, CancellationToken.None);

        await using var read = await factory.CreateDbContextAsync();
        Assert.Equal(2, await read.Ticks.CountAsync());
        var first = await read.Ticks.OrderBy(x => x.Id).FirstAsync();
        Assert.Equal("alpha", first.ExchangeId);
        Assert.Equal("BTC", first.Symbol);
        Assert.Equal(10m, first.Price);
        Assert.Equal("{\"raw\":1}", first.RawPayload);
    }

    private sealed class TestDbContextFactory : IDbContextFactory<TickDbContext>
    {
        private readonly DbContextOptions<TickDbContext> _options;

        public TestDbContextFactory(DbContextOptions<TickDbContext> options)
        {
            _options = options;
        }

        public TickDbContext CreateDbContext() => new(_options);

        public Task<TickDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(CreateDbContext());
    }
}
