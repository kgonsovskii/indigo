using Indigo.Application.Abstractions;
using Indigo.Application.Configuration;
using Indigo.Domain;
using Microsoft.Extensions.Options;

namespace Indigo.Infrastructure.Processing;

public sealed class RecentTickDeduplicator : ITickDeduplicator
{
    private readonly TimeSpan _window;
    private readonly object _gate = new();
    private readonly Dictionary<string, DateTimeOffset> _seen = new(StringComparer.Ordinal);

    public RecentTickDeduplicator(IOptions<IngestionOptions> options)
    {
        _window = TimeSpan.FromMilliseconds(Math.Clamp(options.Value.DeduplicationWindowMs, 50, 60_000));
    }

    public bool IsDuplicate(NormalizedTick tick)
    {
        var key = $"{tick.ExchangeId}\u001f{tick.Symbol}\u001f{tick.Price}\u001f{tick.Volume}\u001f{tick.TimestampUtc.ToUnixTimeMilliseconds()}";
        var now = DateTimeOffset.UtcNow;
        lock (_gate)
        {
            PruneLocked(now);
            if (_seen.ContainsKey(key))
            {
                return true;
            }

            _seen[key] = now;
            return false;
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
