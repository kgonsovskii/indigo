using Microsoft.Extensions.Options;
using Poller.Application;
using Poller.Model;

namespace Poller.Infrastructure;

public sealed class RecentTickDeduplicator : ITickDeduplicator
{
    private readonly TimeSpan _window;
    private readonly TimeProvider _time;
    private readonly Lock _gate = new();
    private readonly Dictionary<string, DateTimeOffset> _seen = new(StringComparer.Ordinal);

    public RecentTickDeduplicator(IOptions<IngestionOptions> options, TimeProvider time)
    {
        _window = options.Value.DeduplicationWindow;
        _time = time;
    }

    public bool IsDuplicate(NormalizedTick tick)
    {
        var key = $"{tick.ExchangeId}-{tick.Symbol}-{tick.Price}-{tick.Volume}-{tick.TimestampUtc.ToUnixTimeMilliseconds()}";
        var now = _time.GetUtcNow();
        lock (_gate)
        {
            PruneLocked(now);
            return !_seen.TryAdd(key, now);
        }
    }

    private void PruneLocked(DateTimeOffset now)
    {
        if (_seen.Count == 0)
        {
            return;
        }

        var threshold = now - _window;
        foreach (var kv in _seen.ToArray())
        {
            if (kv.Value < threshold)
            {
                _seen.Remove(kv.Key);
            }
        }
    }
}
