using System.Text.Json.Serialization;

namespace StockExchange.LaToken;

internal sealed class LaTokenQuoteWire
{
    [JsonPropertyName("pair")]
    public required string Pair { get; init; }

    [JsonPropertyName("price")]
    public required string Price { get; init; }

    [JsonPropertyName("volume")]
    public required string Volume { get; init; }

    [JsonPropertyName("timestamp")]
    public long Timestamp { get; init; }
}
