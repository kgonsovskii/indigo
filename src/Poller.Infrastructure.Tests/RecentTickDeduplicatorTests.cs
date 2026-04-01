using FluentAssertions;
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

    [Theory]
    [InlineData(100, 50, true)]
    [InlineData(100, 101, false)]
    public void SameTickAfterTimeAdvance_DependsOnDeduplicationWindow(
        int deduplicationWindowMs,
        int timeAdvanceMs,
        bool expectThirdCallIsDuplicate)
    {
        var fake = new FakeTimeProvider(FixedTs);
        var window = TimeSpan.FromMilliseconds(deduplicationWindowMs);
        var d = Create(window, fake);
        var tick = new NormalizedTick("LaToken", "BTC-USD", SamplePrice, SampleVolume, FixedTs);

        d.IsDuplicate(tick).Should().BeFalse();
        d.IsDuplicate(tick).Should().BeTrue();

        fake.Advance(TimeSpan.FromMilliseconds(timeAdvanceMs));

        d.IsDuplicate(tick).Should().Be(expectThirdCallIsDuplicate);
    }

    [Fact]
    public void IdenticalTickTwiceSecondIsDuplicate()
    {
        var d = Create(GiantWindow);
        var tick = new NormalizedTick("LaToken", "BTC-USD", SamplePrice, SampleVolume, FixedTs);

        d.IsDuplicate(tick).Should().BeFalse();
        d.IsDuplicate(tick).Should().BeTrue();
    }

    [Fact]
    public void DifferentExchangeNotDuplicate()
    {
        var d = Create(GiantWindow);
        var a = new NormalizedTick("LaToken", "BTC-USD", SamplePrice, SampleVolume, FixedTs);
        var b = new NormalizedTick("CoinBase", "BTC-USD", SamplePrice, SampleVolume, FixedTs);

        d.IsDuplicate(a).Should().BeFalse();
        d.IsDuplicate(b).Should().BeFalse();
    }

    [Fact]
    public void DifferentTimestampMsNotDuplicate()
    {
        var d = Create(GiantWindow);
        var a = new NormalizedTick("LaToken", "BTC-USD", SamplePrice, SampleVolume, FixedTs);
        var b = new NormalizedTick("LaToken", "BTC-USD", SamplePrice, SampleVolume, FixedTs.AddMilliseconds(1));

        d.IsDuplicate(a).Should().BeFalse();
        d.IsDuplicate(b).Should().BeFalse();
    }


    private static RecentTickDeduplicator Create(TimeSpan deduplicationWindow, TimeProvider? time = null)
    {
        var options = Options.Create(new IngestionOptions { DeduplicationWindow = deduplicationWindow });
        return new RecentTickDeduplicator(options, time ?? TimeProvider.System);
    }
}
