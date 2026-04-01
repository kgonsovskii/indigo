using StockExchange.Base;

namespace StockExchange.CoinBase;

internal sealed class Program : ProgramBase
{
    public static Task Main(string[] args) => RunAsync<Program>(args);

    protected override QuoteWebSocketServerBase CreateServer() => new CoinBaseQuoteServer();
}
