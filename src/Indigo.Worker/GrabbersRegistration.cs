using System.Threading.Channels;
using Indigo.Application;
using Indigo.Application.Abstractions;
using Indigo.Application.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StockGrabber;
using StockParser.CoinBase;
using StockParser.LaToken;

namespace Indigo.Worker
{
    public static class GrabbersRegistration
    {
        public static IServiceCollection AddStockGrabbers(this IServiceCollection services, IConfiguration configuration)
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

                if (string.Equals(g.Name, "LaToken", StringComparison.OrdinalIgnoreCase))
                {
                    services.AddSingleton<IHostedService>(sp => CreateLaTokenGrabber(sp, binding));
                }
                else if (string.Equals(g.Name, "CoinBase", StringComparison.OrdinalIgnoreCase))
                {
                    services.AddSingleton<IHostedService>(sp => CreateCoinBaseGrabber(sp, binding));
                }
            }

            return services;
        }

        private static StockGrabber<LaTokenParser> CreateLaTokenGrabber(IServiceProvider sp, GrabberEndpointBinding binding)
        {
            return new StockGrabber<LaTokenParser>(
                sp.GetRequiredService<LaTokenParser>(),
                binding,
                sp.GetRequiredService<ChannelWriter<TickToPersist>>(),
                sp.GetRequiredService<ITickDeduplicator>(),
                sp.GetRequiredService<IOptions<IngestionOptions>>(),
                sp.GetRequiredService<ILogger<StockGrabber<LaTokenParser>>>());
        }

        private static StockGrabber<CoinBaseParser> CreateCoinBaseGrabber(IServiceProvider sp, GrabberEndpointBinding binding)
        {
            return new StockGrabber<CoinBaseParser>(
                sp.GetRequiredService<CoinBaseParser>(),
                binding,
                sp.GetRequiredService<ChannelWriter<TickToPersist>>(),
                sp.GetRequiredService<ITickDeduplicator>(),
                sp.GetRequiredService<IOptions<IngestionOptions>>(),
                sp.GetRequiredService<ILogger<StockGrabber<CoinBaseParser>>>());
        }
    }
}
