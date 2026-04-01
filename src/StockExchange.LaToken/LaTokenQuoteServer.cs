using System.Globalization;
using System.Text.Json;
using StockExchange.Base;

namespace StockExchange.LaToken
{
    internal sealed class LaTokenQuoteServer : QuoteWebSocketServerBase
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = null,
        };

        protected override int ListenPort => 5051;

        public override string ExchangeLabel => "LaToken";

        protected override string BuildQuoteJson(Random random, string symbol)
        {
            var price = (decimal)(random.NextDouble() * 90_000d + 1d);
            var vol = (decimal)(random.NextDouble() * 5d + 0.001d);
            var inv = CultureInfo.InvariantCulture;
            var wire = new LaTokenQuoteWire
            {
                Pair = symbol,
                Price = price.ToString(inv),
                Volume = vol.ToString(inv),
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            };

            return JsonSerializer.Serialize(wire, JsonOptions);
        }
    }
}
