namespace StockExchange.CoinBase
{
    internal static class Program
    {
        public static async Task Main(string[] args)
        {
            var server = new CoinBaseQuoteServer();
            await server.RunAsync(args);
        }
    }
}
