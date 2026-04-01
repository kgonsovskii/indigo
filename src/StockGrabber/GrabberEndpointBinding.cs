namespace StockGrabber;

public sealed class GrabberEndpointBinding
{
    public required string Name { get; init; }

    public required Uri WebSocketUri { get; init; }
}
