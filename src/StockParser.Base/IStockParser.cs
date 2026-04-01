using Indigo.Domain;

namespace StockParser.Base;

public interface IStockParser
{
    bool TryParse(string rawPayload, string feedId, out NormalizedTick? tick);
}
