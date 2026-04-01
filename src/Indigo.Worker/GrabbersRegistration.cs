using Indigo.Application.Configuration;
using StockGrabber;
using StockParser.CoinBase;
using StockParser.LaToken;

namespace Indigo.Worker
{
    public static class GrabbersRegistration
    {
        public static IServiceCollection AddConfiguredStockGrabbers(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<LaTokenParser>();
            services.AddSingleton<CoinBaseParser>();

            var items = configuration.GetSection(GrabbersOptions.SectionName).Get<List<GrabberInstanceOptions>>() ?? new();
            foreach (var g in items)
            {
                if (string.IsNullOrWhiteSpace(g.Name) || string.IsNullOrWhiteSpace(g.WebSocketUri))
                {
                    continue;
                }

                var binding = new GrabberEndpointBinding
                {
                    Name = g.Name.Trim(),
                    WebSocketUri = new Uri(g.WebSocketUri),
                };

                if (string.Equals(g.Parser, "LaToken", StringComparison.OrdinalIgnoreCase))
                {
                    services.AddSingleton<IHostedService>(sp =>
                        ActivatorUtilities.CreateInstance<StockGrabber<LaTokenParser>>(sp, binding));
                }
                else if (string.Equals(g.Parser, "CoinBase", StringComparison.OrdinalIgnoreCase))
                {
                    services.AddSingleton<IHostedService>(sp =>
                        ActivatorUtilities.CreateInstance<StockGrabber<CoinBaseParser>>(sp, binding));
                }
            }

            return services;
        }
    }
}
