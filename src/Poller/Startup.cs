using Feed.Grabber;
using Feed.Parser.Base;
using Feed.Parser.CoinBase;
using Feed.Parser.LaToken;
using Poller.Infrastructure;

namespace Poller;

internal static class Startup
{
    public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddHostedService<DatabaseInitialization>();

        services.AddHostedService<BatchedTickPersistence>();

        services.AddHostedService<MetricsLogging>();

        services.AddPollerServices(configuration);

        services.AddFeedGrabber<LaTokenParser>(configuration);

        services.AddFeedGrabber<CoinBaseParser>(configuration);
    }

    private static void AddFeedGrabber<TParser>(this IServiceCollection services, IConfiguration configuration)
        where TParser : class, IFeedParser
    {
        var label = TParser.ConfigurationSectionKey;
        services.AddOptions<FeedGrabberOptions>(label)
            .Bind(configuration.GetSection($"FeedGrabber:{label}"));

        services.AddTransient<TParser>();

        services.AddTransient<FeedGrabber<TParser>>();

        services.AddHostedService<FeedGrabberHost<TParser>>();
    }
}
