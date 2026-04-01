using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using Poller.Application;
using Poller.Model;
using Xunit;

namespace Poller.Infrastructure.Tests;

public sealed class RecentTickDeduplicatorTests
{
    private static readonly TimeSpan GiantWindow = TimeSpan.FromHours(1);

    private const decimal SamplePrice = 42.5m;

    private const decimal SampleVolume = 1.2m;

    private static readonly DateTimeOffset FixedTs = new(2026, 4, 1, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void First_occurrence_is_not_duplicate()
    {
        var d = Create(GiantWindow);
        var tick = new NormalizedTick("LaToken", "BTC-USD", SamplePrice, SampleVolume, FixedTs);

        Assert.False(d.IsDuplicate(tick));
    }

    [Fact]
    public void Identical_tick_twice_second_is_duplicate()
    {
        var d = Create(GiantWindow);
        var tick = new NormalizedTick("LaToken", "BTC-USD", SamplePrice, SampleVolume, FixedTs);

        Assert.False(d.IsDuplicate(tick));
        Assert.True(d.IsDuplicate(tick));
    }

    [Fact]
    public void Different_exchange_not_duplicate()
    {
        var d = Create(GiantWindow);
        var a = new NormalizedTick("LaToken", "BTC-USD", SamplePrice, SampleVolume, FixedTs);
        var b = new NormalizedTick("CoinBase", "BTC-USD", SamplePrice, SampleVolume, FixedTs);

        Assert.False(d.IsDuplicate(a));
        Assert.False(d.IsDuplicate(b));
    }

    [Fact]
    public void Different_timestamp_ms_not_duplicate()
    {
        var d = Create(GiantWindow);
        var a = new NormalizedTick("LaToken", "BTC-USD", SamplePrice, SampleVolume, FixedTs);
        var b = new NormalizedTick("LaToken", "BTC-USD", SamplePrice, SampleVolume, FixedTs.AddMilliseconds(1));

        Assert.False(d.IsDuplicate(a));
        Assert.False(d.IsDuplicate(b));
    }

    [Fact]
    public void After_deduplication_window_entry_expires()
    {
        var fake = new FakeTimeProvider(FixedTs);
        var d = Create(GiantWindow, fake);
        var tick = new NormalizedTick("LaToken", "BTC-USD", SamplePrice, SampleVolume, FixedTs);

        Assert.False(d.IsDuplicate(tick));
        Assert.True(d.IsDuplicate(tick));

        fake.Advance(GiantWindow + TimeSpan.FromTicks(1));

        Assert.False(d.IsDuplicate(tick));
    }

    private static RecentTickDeduplicator Create(TimeSpan deduplicationWindow, TimeProvider? time = null)
    {
        var options = Options.Create(new IngestionOptions { DeduplicationWindow = deduplicationWindow });
        return new RecentTickDeduplicator(options, time ?? TimeProvider.System);
    }
}
