using StockExchange.Base;

namespace StockExchange.LaToken;

internal sealed class Program : ProgramBase
{
    public static Task Main(string[] args) => RunAsync<Program>(args);

    protected override QuoteWebSocketServerBase CreateServer() => new LaTokenQuoteServer();
}
