using System.Threading.Channels;
using Indigo.Application;
using Indigo.Application.Abstractions;
using Indigo.Application.Configuration;
using Indigo.Infrastructure.Hosting;
using Indigo.Infrastructure.Persistence;
using Indigo.Infrastructure.Processing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Indigo.Infrastructure.DependencyInjection;

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
