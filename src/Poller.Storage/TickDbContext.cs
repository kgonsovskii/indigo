using Microsoft.EntityFrameworkCore;
using Poller.Model;

namespace Poller.Storage;

public sealed class TickDbContext : DbContext
{
    public TickDbContext(DbContextOptions<TickDbContext> options)
        : base(options)
    {
    }

    public DbSet<TickRecord> Ticks => Set<TickRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var e = modelBuilder.Entity<TickRecord>();
        e.HasKey(x => x.Id);
        e.Property(x => x.ExchangeId).HasMaxLength(64).IsRequired();
        e.Property(x => x.Symbol).HasMaxLength(64).IsRequired();
        e.Property(x => x.Price).HasPrecision(28, 12);
        e.Property(x => x.Volume).HasPrecision(28, 12);
        e.HasIndex(x => x.TimestampUtc);
        e.HasIndex(x => new { x.ExchangeId, x.Symbol, x.TimestampUtc });
    }
}
