namespace StockExchange.LaToken
{
    internal static class Program
    {
        public static async Task Main(string[] args)
        {
            var server = new LaTokenQuoteServer();
            await server.RunAsync(args);
        }
    }
}
