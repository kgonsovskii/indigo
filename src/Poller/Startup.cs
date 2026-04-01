using Poller.Infrastructure.DependencyInjection;
using StockGrabber;
using StockParser.Base;
using StockParser.CoinBase;
using StockParser.LaToken;

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
        where TParser : class, IStockParser
    {
        var label = TParser.ConfigurationSectionKey;
        services.AddOptions<StockGrabberOptions>(label)
            .Bind(configuration.GetSection($"StockGrabber:{label}"));

        services.AddTransient<TParser>();

        services.AddTransient<StockGrabber<TParser>>();

        services.AddHostedService<StockGrabberHost<TParser>>();
    }
}
