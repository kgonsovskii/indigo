using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
        services.AddOptions<StockGrabberOptions>(StockGrabberOptionsKeys.LaToken)
            .Bind(configuration.GetSection($"{StockGrabberSection}:{StockGrabberOptionsKeys.LaToken}"));
        services.AddSingleton<LaTokenParser>();
        services.AddHostedService<StockGrabberHost<LaTokenParser>>();

        services.AddOptions<StockGrabberOptions>(StockGrabberOptionsKeys.CoinBase)
            .Bind(configuration.GetSection($"{StockGrabberSection}:{StockGrabberOptionsKeys.CoinBase}"));
        services.AddSingleton<CoinBaseParser>();
        services.AddSingleton<StockGrabber<CoinBaseParser>>();
        services.AddHostedService<StockGrabberHost<CoinBaseParser>>();

        return services;
    }

    private static IServiceCollection AddStockGrabber<TParser>(this IServiceCollection services, string label, IConfiguration configuration) where TParser : class, IStockParser
    {
        services.AddOptions<StockGrabberOptions>(label)
            .Bind(configuration.GetSection($"{StockGrabberSection}:{label}"));
        services.AddSingleton<TParser>();
        services.AddHostedService<StockGrabberHost<TParser>>();
    }
}
