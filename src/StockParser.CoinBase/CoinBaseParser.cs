using System.Globalization;
using System.Text.Json;
using Indigo.Domain;
using StockParser.Base;

namespace StockParser.CoinBase;

public sealed class CoinBaseParser : IStockParser
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public bool TryParse(string rawPayload, string feedId, out NormalizedTick? tick)
    {
        tick = null;
        try
        {
            var m = JsonSerializer.Deserialize<CoinbaseMsg>(rawPayload, JsonOptions);
            if (m?.ProductId is null || m.Price is null || m.LastSize is null || m.Time is null)
            {
                return false;
            }

            if (!string.Equals(m.Type, "ticker", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!decimal.TryParse(m.Price, NumberStyles.Any, CultureInfo.InvariantCulture, out var price))
            {
                return false;
            }

            if (!decimal.TryParse(m.LastSize, NumberStyles.Any, CultureInfo.InvariantCulture, out var vol))
            {
                return false;
            }

            if (!DateTimeOffset.TryParse(m.Time, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var ts))
            {
                return false;
            }

            tick = new NormalizedTick(feedId, m.ProductId, price, vol, ts);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
