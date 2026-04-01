using System.Threading.Channels;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Poller.Application;
using Poller.Model;
using Poller.Storage;

namespace Poller.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPollerServices(this IServiceCollection services, IConfiguration configuration)
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

        var connectionString = configuration.GetConnectionString("Ticks")
            ?? throw new InvalidOperationException(
                "Connection string 'Ticks' is not configured. Set ConnectionStrings:Ticks (see appsettings.json).");
        services.AddDbContextFactory<TickDbContext>(o => o.UseSqlite(PrepareSqliteTicksConnectionString(connectionString)));

        services.AddSingleton<ITickPersistence, EfTickPersistence>();
        services.AddSingleton<ITickDeduplicator, RecentTickDeduplicator>();
        services.AddSingleton<ITickMetrics, TickMetrics>();

        return services;
    }

    private static string PrepareSqliteTicksConnectionString(string connectionString)
    {
        var builder = new SqliteConnectionStringBuilder(connectionString);
        var path = builder.DataSource;
        if (string.IsNullOrWhiteSpace(path))
        {
            return connectionString;
        }

        if (!Path.IsPathRooted(path))
        {
            path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, path));
            builder.DataSource = path;
        }

        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        return builder.ConnectionString;
    }
}
