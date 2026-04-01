using System.Globalization;
using StockExchange.Base;

namespace StockExchange.CoinBase
{
    internal sealed class CoinBaseQuoteServer : QuoteWebSocketServerBase
    {
        protected override int ListenPort => 5052;

        public override string ExchangeLabel => "CoinBase";

        protected override string[] Symbols { get; } = ["BTC-USD", "ETH-USD", "XRP-USD", "SOL-USD"];

        protected override string BuildQuoteJson(Random random, string symbol)
        {
            var price = (decimal)(random.NextDouble() * 90_000d + 1d);
            var lastSize = (decimal)(random.NextDouble() * 5d + 0.001d);
            var time = DateTimeOffset.UtcNow.ToString("o", CultureInfo.InvariantCulture);
            var inv = CultureInfo.InvariantCulture;
            return $"{{\"type\":\"ticker\",\"product_id\":\"{symbol}\",\"price\":\"{price.ToString(inv)}\",\"last_size\":\"{lastSize.ToString(inv)}\",\"time\":\"{time}\"}}";
        }
    }
}
