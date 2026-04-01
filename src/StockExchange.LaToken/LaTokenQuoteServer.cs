using System.Globalization;
using StockExchange.Base;

namespace StockExchange.LaToken
{
    internal sealed class LaTokenQuoteServer : QuoteWebSocketServerBase
    {
        protected override int ListenPort => 5051;

        public override string ExchangeLabel => "LaToken";

        protected override string BuildQuoteJson(Random random, string symbol)
        {
            var price = (decimal)(random.NextDouble() * 90_000d + 1d);
            var vol = (decimal)(random.NextDouble() * 5d + 0.001d);
            var t = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var inv = CultureInfo.InvariantCulture;
            return $"{{\"pair\":\"{symbol}\",\"price\":\"{price.ToString(inv)}\",\"volume\":\"{vol.ToString(inv)}\",\"timestamp\":{t}}}";
        }
    }
}
