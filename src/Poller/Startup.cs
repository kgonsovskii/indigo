using Feed.Grabber;
using Feed.Parser.CoinBase;
using Feed.Parser.LaToken;
using Poller.Infrastructure.DependencyInjection;
using StockParser.Base;

namespace Poller;

internal static class Startup
{
    public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddIndigoMarketData(configuration);

        services.AddStockGrabber<LaTokenParser>(configuration);

        services.AddStockGrabber<CoinBaseParser>(configuration);
    }

    private static void AddStockGrabber<TParser>(this IServiceCollection services, IConfiguration configuration)
        where TParser : class, IFeedParser
    {
        var label = TParser.ConfigurationSectionKey;
        services.AddOptions<FeedGrabberOptions>(label)
            .Bind(configuration.GetSection($"Feed.Grabber:{label}"));

        services.AddTransient<TParser>();

        services.AddTransient<FeedGrabber<TParser>>();

        services.AddHostedService<FeedGrabberHost<TParser>>();
    }
}
