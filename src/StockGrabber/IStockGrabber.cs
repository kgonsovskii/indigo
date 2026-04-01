namespace StockGrabber;

public interface IStockGrabber
{
    Task RunAsync(CancellationToken cancellationToken);
}
