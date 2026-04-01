using Poller.Domain;

namespace StockParser.Base;

public interface IStockParser
{
    static abstract string ConfigurationSectionKey { get; }

    bool TryParse(string rawPayload, string feedId, out NormalizedTick? tick);
}
