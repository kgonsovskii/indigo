using Feed.Parser.CoinBase;
using Feed.Parser.LaToken;
using FluentAssertions;
using Poller.Model;
using Xunit;

namespace Feed.Parser.Tests;

public sealed class NormalizedTickTests
{
    private const string Pair = "BTC-USD";

    private const decimal ExpectedPrice = 42000.50m;

    private const decimal ExpectedVolume = 1.25m;

    private static readonly DateTimeOffset ExpectedTimestamp =
        DateTimeOffset.FromUnixTimeMilliseconds(1_775_044_800_000L);

    private const string LaTokenExchangeId = "LaToken";

    private const string CoinBaseExchangeId = "CoinBase";

    [Fact]
    public void EquivalentWirePayloadsAlignOnSymbolPriceVolumeAndTimestamp()
    {
        var testDataDir = Path.Combine(AppContext.BaseDirectory, "TestData");

        var parseRuns = new (string FileName, string ExchangeId, object Parser)[]
        {
            ("latoken-tick.json", LaTokenExchangeId, new LaTokenParser()),
            ("coinbase-tick.json", CoinBaseExchangeId, new CoinBaseParser()),
        };

        foreach (var (fileName, exchangeId, parser) in parseRuns)
        {
            var json = File.ReadAllText(Path.Combine(testDataDir, fileName)).Trim();
            var (ok, tick) = TryParseConcreteParser(parser, json, exchangeId);
            ok.Should().BeTrue();
            tick.Should().NotBeNull();
            AssertMatchesFixture(tick!, exchangeId);
        }
    }

    private static (bool Ok, NormalizedTick? Tick) TryParseConcreteParser(object parser, string json, string exchangeId)
    {
        dynamic p = parser;
        bool ok = p.TryParse(json, exchangeId, out NormalizedTick? tick);
        return (ok, tick);
    }

    private static void AssertMatchesFixture(NormalizedTick tick, string expectedExchangeId)
    {
        tick.Symbol.Should().Be(Pair);
        tick.Price.Should().Be(ExpectedPrice);
        tick.Volume.Should().Be(ExpectedVolume);
        tick.TimestampUtc.Should().Be(ExpectedTimestamp);
        tick.ExchangeId.Should().Be(expectedExchangeId);
    }
}
