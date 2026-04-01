using StockGrabber;
using StockParser.Base;
using StockParser.CoinBase;
using StockParser.LaToken;

namespace Indigo.Worker;

public static class GrabbersRegistration
{
    private const string StockGrabberSection = "StockGrabber";

    public static IServiceCollection AddStockGrabbers(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddStockGrabber<LaTokenParser>("LaToken", configuration);

        services.AddStockGrabber<CoinBaseParser>("CoinBase", configuration);

        return services;
    }

    private static void AddStockGrabber<TParser>(this IServiceCollection services, string label, IConfiguration configuration) where TParser : class, IStockParser
    {
        services.AddOptions<StockGrabberOptions>(label)
            .Bind(configuration.GetSection($"{StockGrabberSection}:{label}"));
        services.AddSingleton<TParser>();
        services.AddHostedService<StockGrabberHost<TParser>>();
    }
}
