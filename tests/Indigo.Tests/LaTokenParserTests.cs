using LaTokenParser;

namespace Indigo.Tests;

public sealed class LaTokenParserTests
{
    private readonly LaTokenParser.LaTokenParser _parser = new();

    [Fact]
    public void TryParse_valid_returns_tick_with_feed_id()
    {
        var json = """{"pair":"BTC_USDT","price":"50123.4","volume":"0.01","timestamp":1700000000000}""";
        var ok = _parser.TryParse(json, "FeedA", out var tick);
        Assert.True(ok);
        Assert.NotNull(tick);
        Assert.Equal("FeedA", tick!.ExchangeId);
        Assert.Equal("BTC_USDT", tick.Symbol);
        Assert.Equal(50123.4m, tick.Price);
        Assert.Equal(0.01m, tick.Volume);
    }

    [Fact]
    public void TryParse_invalid_returns_false()
    {
        var ok = _parser.TryParse("{}", "FeedA", out var tick);
        Assert.False(ok);
        Assert.Null(tick);
    }
}
