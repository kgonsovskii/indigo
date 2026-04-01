using CoinBaseParser;

namespace Indigo.Tests;

public sealed class CoinBaseParserTests
{
    private readonly CoinBaseParser.CoinBaseParser _parser = new();

    [Fact]
    public void TryParse_valid_returns_tick_with_feed_id()
    {
        var json = """{"type":"ticker","product_id":"ETH-USD","price":"3200.5","last_size":"1.2","time":"2024-01-15T10:00:00.0000000Z"}""";
        var ok = _parser.TryParse(json, "CoinBaseMain", out var tick);
        Assert.True(ok);
        Assert.NotNull(tick);
        Assert.Equal("CoinBaseMain", tick!.ExchangeId);
        Assert.Equal("ETH-USD", tick.Symbol);
        Assert.Equal(3200.5m, tick.Price);
        Assert.Equal(1.2m, tick.Volume);
    }

    [Fact]
    public void TryParse_wrong_type_returns_false()
    {
        var json = """{"type":"snapshot","product_id":"ETH-USD","price":"1","last_size":"1","time":"2024-01-15T10:00:00Z"}""";
        var ok = _parser.TryParse(json, "x", out var tick);
        Assert.False(ok);
        Assert.Null(tick);
    }
}
