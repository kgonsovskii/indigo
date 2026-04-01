using System.Text.Json.Serialization;

namespace StockParser.LaToken;

internal sealed class LaTokenMsg
{
    [JsonPropertyName("pair")]
    public string? Pair { get; set; }

    [JsonPropertyName("price")]
    public string? Price { get; set; }

    [JsonPropertyName("volume")]
    public string? Volume { get; set; }

    [JsonPropertyName("timestamp")]
    public long? Timestamp { get; set; }
}
