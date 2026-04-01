using System.Diagnostics.Metrics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Poller.Application;

namespace Poller.Infrastructure;

public sealed class PollerInstrumentation : ITickMetrics, IFeedConnectionTelemetry, IDisposable
{
    private readonly ILogger<PollerInstrumentation> _logger;
    private readonly Meter _meter;
    private readonly Counter<long> _ticksPersisted;
    private readonly UpDownCounter<long> _feedLanesActive;
    private readonly TimeSpan _metricsRateWindow;
    private long _persistedTotal;
    private readonly Lock _rateLock = new();
    private readonly Queue<(long UtcTicks, int Count)> _persistRateSamples = new();
    private readonly Lock _exchangeTotalsLock = new();
    private readonly Dictionary<string, long> _persistedByExchange = new(StringComparer.Ordinal);
    private readonly Lock _activeLanesLock = new();
    private readonly Dictionary<string, int> _activeLaneCountByFeed = new(StringComparer.Ordinal);

    public PollerInstrumentation(IOptions<IngestionOptions> ingestion, ILogger<PollerInstrumentation> logger)
    {
        _logger = logger;
        _metricsRateWindow = ingestion.Value.MetricsRateWindow;
        _meter = new Meter("Indigo.Poller", "1.0.0");
        _ticksPersisted = _meter.CreateCounter<long>(
            "poller.ticks.persisted",
            unit: "{tick}",
            description: "Ticks successfully persisted to storage.");
        _feedLanesActive = _meter.CreateUpDownCounter<long>(
            "poller.feed.lanes_connected",
            unit: "{lane}",
            description: "Open WebSocket receive lanes per feed.");
        _meter.CreateObservableGauge(
            "poller.ticks.persisted.per_second",
            ObserveRecentRate,
            unit: "1/s",
            description: "Recent tick persistence rate (sliding window).");
    }

    private IEnumerable<Measurement<double>> ObserveRecentRate()
    {
        yield return new Measurement<double>(RecentPersistRatePerSecond);
    }

    public long PersistedTotal => Interlocked.Read(ref _persistedTotal);

    public double RecentPersistRatePerSecond
    {
        get
        {
            lock (_rateLock)
            {
                var now = DateTime.UtcNow;
                TrimRateWindow(now);
                if (_persistRateSamples.Count == 0)
                {
                    return 0;
                }

                long sum = 0;
                foreach (var e in _persistRateSamples)
                {
                    sum += e.Count;
                }

                var oldest = new DateTime(_persistRateSamples.Peek().UtcTicks, DateTimeKind.Utc);
                var seconds = Math.Max(0.001, (now - oldest).TotalSeconds);
                return sum / seconds;
            }
        }
    }

    public IReadOnlyDictionary<string, long> PersistedTotalByExchange
    {
        get
        {
            lock (_exchangeTotalsLock)
            {
                return new Dictionary<string, long>(_persistedByExchange, StringComparer.Ordinal);
            }
        }
    }

    public void AddPersisted(long count, IReadOnlyDictionary<string, int>? perExchange = null)
    {
        if (count <= 0)
        {
            return;
        }

        Interlocked.Add(ref _persistedTotal, count);

        if (perExchange is not null && perExchange.Count > 0)
        {
            foreach (var pair in perExchange)
            {
                if (pair.Value <= 0)
                {
                    continue;
                }

                _ticksPersisted.Add(
                    pair.Value,
                    [new KeyValuePair<string, object?>("exchange", pair.Key)]);
                lock (_exchangeTotalsLock)
                {
                    _persistedByExchange.TryGetValue(pair.Key, out var t);
                    _persistedByExchange[pair.Key] = t + pair.Value;
                }
            }
        }
        else
        {
            _ticksPersisted.Add(count);
        }

        var now = DateTime.UtcNow;
        var slice = (int)Math.Min(int.MaxValue, count);
        lock (_rateLock)
        {
            TrimRateWindow(now);
            _persistRateSamples.Enqueue((now.Ticks, slice));
        }
    }

    private void TrimRateWindow(DateTime nowUtc)
    {
        var cutoff = (nowUtc - _metricsRateWindow).Ticks;
        while (_persistRateSamples.Count > 0 && _persistRateSamples.Peek().UtcTicks < cutoff)
        {
            _persistRateSamples.Dequeue();
        }
    }

    public void RecordConnected(string feedName, int laneIndex, string parserName, Uri webSocketUri)
    {
        _feedLanesActive.Add(1, new KeyValuePair<string, object?>("feed", feedName));
        lock (_activeLanesLock)
        {
            _activeLaneCountByFeed.TryGetValue(feedName, out var n);
            _activeLaneCountByFeed[feedName] = n + 1;
        }

        _logger.LogInformation(
            "Feed connected {Feed} lane {Lane} parser {Parser} {Uri}",
            feedName,
            laneIndex,
            parserName,
            webSocketUri);
    }

    public void RecordDisconnected(string feedName, int laneIndex)
    {
        _feedLanesActive.Add(-1, new KeyValuePair<string, object?>("feed", feedName));
        lock (_activeLanesLock)
        {
            if (_activeLaneCountByFeed.TryGetValue(feedName, out var n))
            {
                n = Math.Max(0, n - 1);
                if (n == 0)
                {
                    _activeLaneCountByFeed.Remove(feedName);
                }
                else
                {
                    _activeLaneCountByFeed[feedName] = n;
                }
            }
        }

        _logger.LogInformation("Feed disconnected {Feed} lane {Lane}", feedName, laneIndex);
    }

    public void RecordConnectionError(string feedName, int laneIndex, Exception exception, TimeSpan reconnectDelay)
    {
        _logger.LogWarning(
            "Feed connection error {Feed} lane {Lane}; reconnect in {ReconnectDelayMs} ms: {ExceptionType} {Message}",
            feedName,
            laneIndex,
            (long)reconnectDelay.TotalMilliseconds,
            exception.GetType().Name,
            exception.Message);
    }

    public void RecordLaneCrashed(string feedName, int laneIndex, Exception exception, TimeSpan restartDelay)
    {
        _logger.LogError(
            exception,
            "Feed lane crashed {Feed} lane {Lane}; restarting after {RestartDelayMs} ms: {Message}",
            feedName,
            laneIndex,
            (long)restartDelay.TotalMilliseconds,
            exception.Message);
    }

    public IReadOnlyDictionary<string, int> GetActiveLanesSnapshot()
    {
        lock (_activeLanesLock)
        {
            return new Dictionary<string, int>(_activeLaneCountByFeed, StringComparer.Ordinal);
        }
    }

    public void Dispose()
    {
        _meter.Dispose();
    }
}
