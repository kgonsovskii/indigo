using System.Threading.Channels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Poller.Application;
using Poller.Application.Abstractions;
using Poller.Application.Configuration;
using Poller.Infrastructure.Hosting;
using Poller.Infrastructure.Persistence;
using Poller.Infrastructure.Processing;

namespace Poller.Infrastructure.DependencyInjection;

public static class IndigoServiceCollectionExtensions
{
    public static IServiceCollection AddIndigoMarketData(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<IngestionOptions>(configuration.GetSection(IngestionOptions.SectionName));

        var channel = Channel.CreateUnbounded<TickToPersist>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false,
        });

        services.AddSingleton(channel);
        services.AddSingleton<ChannelReader<TickToPersist>>(_ => channel.Reader);
        services.AddSingleton<ChannelWriter<TickToPersist>>(_ => channel.Writer);

        var connectionString = configuration.GetConnectionString("Ticks") ?? "Data Source=ticks.db";
        services.AddDbContextFactory<TickDbContext>(o => o.UseSqlite(connectionString));

        services.AddSingleton<ITickPersistence, EfTickPersistence>();
        services.AddSingleton<ITickDeduplicator, RecentTickDeduplicator>();
        services.AddSingleton<ITickMetrics, TickMetrics>();

        services.AddHostedService<DatabaseInitializationHostedService>();
        services.AddHostedService<BatchedTickPersistenceHostedService>();
        services.AddHostedService<TickMetricsLoggingHostedService>();

        return services;
    }
}
