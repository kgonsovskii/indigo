using Indigo.Infrastructure.DependencyInjection;

namespace Indigo.Worker
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            builder.Services.AddIndigoMarketData(builder.Configuration);
            builder.Services.AddConfiguredStockGrabbers(builder.Configuration);
            var host = builder.Build();
            host.Run();
        }
    }
}
