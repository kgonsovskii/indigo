using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Poller.Model;
using Xunit;

namespace Poller.Storage.Tests;

public sealed class EfTickPersistenceTests
{
    [Fact]
    public async Task SaveBatchAsyncPersistsTicksAndSetsIngestedAtUtc()
    {
        await using var harness = await SqliteHarness.CreateAsync();
        var sut = new EfTickPersistence(harness.Factory);
        var ts = new DateTimeOffset(2026, 4, 1, 12, 0, 0, TimeSpan.Zero);
        var tick = new NormalizedTick("LaToken", "BTC-USD", 42.5m, 1.2m, ts);
        var batch = new[] { new TickToPersist(tick, "{\"raw\":1}") };

        await sut.SaveBatchAsync(batch, CancellationToken.None);

        await using var db = await harness.Factory.CreateDbContextAsync();
        var rows = await db.Ticks.OrderBy(t => t.Id).ToListAsync();
        rows.Should().ContainSingle();
        var row = rows[0];
        row.Id.Should().NotBe(0);
        row.ExchangeId.Should().Be("LaToken");
        row.Symbol.Should().Be("BTC-USD");
        row.Price.Should().Be(42.5m);
        row.Volume.Should().Be(1.2m);
        row.TimestampUtc.Should().Be(ts);
        row.RawPayload.Should().Be("{\"raw\":1}");
        row.IngestedAtUtc.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task SaveBatchAsyncMultipleTicksAllPersisted()
    {
        await using var harness = await SqliteHarness.CreateAsync();
        var sut = new EfTickPersistence(harness.Factory);
        var ts = DateTimeOffset.UtcNow;
        var batch = new TickToPersist[]
        {
            new(new NormalizedTick("A", "X", 1m, 2m, ts), "r1"),
            new(new NormalizedTick("B", "Y", 3m, 4m, ts.AddMinutes(1)), null),
        };

        await sut.SaveBatchAsync(batch, CancellationToken.None);

        await using var db = await harness.Factory.CreateDbContextAsync();
        var rows = await db.Ticks.OrderBy(t => t.ExchangeId).ToListAsync();
        rows.Should().HaveCount(2);
        rows[0].ExchangeId.Should().Be("A");
        rows[0].RawPayload.Should().Be("r1");
        rows[1].ExchangeId.Should().Be("B");
        rows[1].RawPayload.Should().BeNull();
    }

    private sealed class SqliteHarness : IAsyncDisposable
    {
        private readonly SqliteConnection _connection;

        private SqliteHarness(SqliteConnection connection, IDbContextFactory<TickDbContext> factory)
        {
            _connection = connection;
            Factory = factory;
        }

        public IDbContextFactory<TickDbContext> Factory { get; }

        public static async Task<SqliteHarness> CreateAsync()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();
            var options = new DbContextOptionsBuilder<TickDbContext>()
                .UseSqlite(connection)
                .Options;
            await using (var db = new TickDbContext(options))
            {
                await db.Database.EnsureCreatedAsync();
            }

            return new SqliteHarness(connection, new TestTickDbContextFactory(options));
        }

        public ValueTask DisposeAsync()
        {
            _connection.Dispose();
            return ValueTask.CompletedTask;
        }
    }

    private sealed class TestTickDbContextFactory : IDbContextFactory<TickDbContext>
    {
        private readonly DbContextOptions<TickDbContext> _options;

        public TestTickDbContextFactory(DbContextOptions<TickDbContext> options) => _options = options;

        public TickDbContext CreateDbContext() => new(_options);
    }
}
