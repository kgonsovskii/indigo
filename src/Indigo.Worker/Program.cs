namespace Indigo.Worker
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            builder.Services.AddStockGrabbers(builder.Configuration);
            var host = builder.Build();
            host.Run();
        }
    }
}
