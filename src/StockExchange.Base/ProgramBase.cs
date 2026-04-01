namespace StockExchange.Base;

public abstract class ProgramBase
{
    protected abstract QuoteWebSocketServerBase CreateServer();

    private async Task RunCoreAsync(string[] args)
    {
        var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };

        await CreateServer().RunAsync(args, cts.Token);
    }

    protected static Task RunAsync<TProgram>(string[] args)
        where TProgram : ProgramBase, new()
    {
        var program = new TProgram();
        return program.RunCoreAsync(args);
    }
}
