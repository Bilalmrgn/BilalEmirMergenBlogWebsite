using System.Threading.Channels;
using BilalEmirMergenWebsite.Data;
using BilalEmirMergenWebsite.Models;

namespace BilalEmirMergenWebsite.Services;

public interface IAnalyticsQueue
{
    void Enqueue(Analytics item);
}

public class AnalyticsQueue : IAnalyticsQueue
{
    private readonly Channel<Analytics> _channel = Channel.CreateUnbounded<Analytics>(new UnboundedChannelOptions
    {
        SingleReader = true
    });

    public void Enqueue(Analytics item)
    {
        _channel.Writer.TryWrite(item);
    }

    public ChannelReader<Analytics> Reader => _channel.Reader;
}

public class AnalyticsBackgroundProcessor : BackgroundService
{
    private readonly AnalyticsQueue _queue;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AnalyticsBackgroundProcessor> _logger;

    public AnalyticsBackgroundProcessor(IAnalyticsQueue queue, IServiceProvider serviceProvider, ILogger<AnalyticsBackgroundProcessor> logger)
    {
        _queue = (AnalyticsQueue)queue;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var batch = new List<Analytics>();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (await _queue.Reader.WaitToReadAsync(stoppingToken))
                {
                    while (_queue.Reader.TryRead(out var item))
                    {
                        batch.Add(item);
                        if (batch.Count >= 20) break;
                    }

                    if (batch.Count > 0)
                    {
                        using var scope = _serviceProvider.CreateScope();
                        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                        dbContext.Analytics.AddRange(batch);
                        await dbContext.SaveChangesAsync(stoppingToken);
                        batch.Clear();
                    }
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing background analytics batch.");
                batch.Clear();
                await Task.Delay(2000, stoppingToken);
            }
        }
    }
}
