using System.Globalization;
using System.Text.Json;
using Poller.Domain;
using StockParser.Base;

namespace StockParser.LaToken;

public sealed class LaTokenParser : IStockParser
{
    public static string ConfigurationSectionKey => "LaToken";

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public bool TryParse(string rawPayload, string feedId, out NormalizedTick? tick)
    {
        tick = null;
        try
        {
            var m = JsonSerializer.Deserialize<LaTokenMsg>(rawPayload, JsonOptions);
            if (m?.Pair is null || m.Price is null || m.Volume is null || !m.Timestamp.HasValue)
            {
                return false;
            }

            if (!decimal.TryParse(m.Price, NumberStyles.Any, CultureInfo.InvariantCulture, out var price))
            {
                return false;
            }

            if (!decimal.TryParse(m.Volume, NumberStyles.Any, CultureInfo.InvariantCulture, out var vol))
            {
                return false;
            }

            var ts = DateTimeOffset.FromUnixTimeMilliseconds(m.Timestamp.Value);
            tick = new NormalizedTick(feedId, m.Pair, price, vol, ts);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
