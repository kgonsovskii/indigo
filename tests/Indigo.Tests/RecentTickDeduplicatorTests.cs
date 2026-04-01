using Indigo.Application.Configuration;
using Indigo.Domain;
using Indigo.Infrastructure.Processing;
using Microsoft.Extensions.Options;

namespace Indigo.Tests;

public sealed class RecentTickDeduplicatorTests
{
    [Fact]
    public void IsDuplicate_same_fingerprint_within_window_true()
    {
        var opt = Options.Create(new IngestionOptions { DeduplicationWindowMs = 60_000 });
        var d = new RecentTickDeduplicator(opt);
        var t = new NormalizedTick("alpha", "BTC", 1m, 2m, DateTimeOffset.UtcNow);
        Assert.False(d.IsDuplicate(t));
        Assert.True(d.IsDuplicate(t));
    }

    [Fact]
    public void IsDuplicate_different_price_not_duplicate()
    {
        var opt = Options.Create(new IngestionOptions { DeduplicationWindowMs = 60_000 });
        var d = new RecentTickDeduplicator(opt);
        var a = new NormalizedTick("alpha", "BTC", 1m, 2m, DateTimeOffset.UtcNow);
        var b = new NormalizedTick("alpha", "BTC", 1.1m, 2m, DateTimeOffset.UtcNow);
        Assert.False(d.IsDuplicate(a));
        Assert.False(d.IsDuplicate(b));
    }
}
