using Poller.Model;

namespace Feed.Parser.Base;

public interface IFeedParser
{
    static abstract string ConfigurationSectionKey { get; }

    bool TryParse(string rawPayload, string feedId, out NormalizedTick? tick);
}
