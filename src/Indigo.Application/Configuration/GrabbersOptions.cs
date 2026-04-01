namespace Indigo.Application.Configuration;

public sealed class GrabberInstanceOptions
{
    public string Name { get; set; } = string.Empty;

    public string WebSocketUri { get; set; } = string.Empty;
}

public static class GrabbersOptions
{
    public const string SectionName = "Grabbers";
}
