using System.Globalization;
using System.Text.Json;
using StockExchange.Base;

namespace StockExchange.CoinBase
{
    internal sealed class CoinBaseQuoteServer : QuoteWebSocketServerBase
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = null,
        };

        protected override int ListenPort => 5052;

        public override string ExchangeLabel => "CoinBase";

        protected override string[] Symbols { get; } = ["BTC-USD", "ETH-USD", "XRP-USD", "SOL-USD"];

        protected override string BuildQuoteJson(Random random, string symbol)
        {
            var price = (decimal)(random.NextDouble() * 90_000d + 1d);
            var lastSize = (decimal)(random.NextDouble() * 5d + 0.001d);
            var inv = CultureInfo.InvariantCulture;
            var wire = new CoinBaseTickerWire
            {
                Type = "ticker",
                ProductId = symbol,
                Price = price.ToString(inv),
                LastSize = lastSize.ToString(inv),
                Time = DateTimeOffset.UtcNow.ToString("o", inv),
            };

            return JsonSerializer.Serialize(wire, JsonOptions);
        }
    }
}
