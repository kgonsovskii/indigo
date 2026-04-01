namespace Feed.Grabber;

public sealed class FeedGrabberOptions
{
    public string Name { get; set; } = string.Empty;

    public string WebSocketUri { get; set; } = string.Empty;

    public int DegreeOfParallelism { get; set; } = 1;
}
