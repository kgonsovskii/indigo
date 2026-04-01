namespace Poller;

internal static class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        Startup.ConfigureServices(builder.Services, builder.Configuration);
        builder.Build().Run();
    }
}
