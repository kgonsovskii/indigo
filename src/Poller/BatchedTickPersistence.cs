using System.Threading.Channels;
using Microsoft.Extensions.Options;
using Poller.Application;
using Poller.Model;

namespace Poller;

public sealed class BatchedTickPersistence : BackgroundService
{
    private readonly ChannelReader<TickToPersist> _reader;
    private readonly ITickPersistence _persistence;
    private readonly ITickMetrics _metrics;
    private readonly IOptions<IngestionOptions> _options;
    private readonly ILogger<BatchedTickPersistence> _logger;

    public BatchedTickPersistence(
        ChannelReader<TickToPersist> reader,
        ITickPersistence persistence,
        ITickMetrics metrics,
        IOptions<IngestionOptions> options,
        ILogger<BatchedTickPersistence> logger)
    {
        _reader = reader;
        _persistence = persistence;
        _metrics = metrics;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var opt = _options.Value;
        var batch = new List<TickToPersist>(opt.BatchSize);
        while (!stoppingToken.IsCancellationRequested)
        {
            await _reader.WaitToReadAsync(stoppingToken);
            batch.Clear();
            var deadline = DateTime.UtcNow.AddMilliseconds(Math.Max(1, opt.BatchMaxWaitMs));
            while (batch.Count < opt.BatchSize && DateTime.UtcNow < deadline)
            {
                while (batch.Count < opt.BatchSize && _reader.TryRead(out var item))
                {
                    batch.Add(item);
                }

                if (batch.Count >= opt.BatchSize)
                {
                    break;
                }

                if (!_reader.TryPeek(out _))
                {
                    await Task.Delay(1, stoppingToken);
                }
            }

            if (batch.Count == 0)
            {
                continue;
            }

            try
            {
                await _persistence.SaveBatchAsync(batch, stoppingToken);
                _metrics.AddPersisted(batch.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Batch persistence failed size {Size}", batch.Count);
                foreach (var item in batch)
                {
                    try
                    {
                        await _persistence.SaveBatchAsync([item], stoppingToken);
                        _metrics.AddPersisted(1);
                    }
                    catch (Exception ex2)
                    {
                        _logger.LogError(ex2, "Single tick persistence failed {Exchange} {Symbol}", item.Tick.ExchangeId, item.Tick.Symbol);
                    }
                }
            }
        }
    }
}
