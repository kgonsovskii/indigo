namespace StockGrabber;

public sealed class StockGrabberOptions
{
    public string Name { get; set; } = string.Empty;

    public string WebSocketUri { get; set; } = string.Empty;

    public int DegreeOfParallelism { get; set; } = 1;
}
