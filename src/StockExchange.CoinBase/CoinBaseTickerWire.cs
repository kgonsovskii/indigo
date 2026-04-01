using System.Text.Json.Serialization;

namespace StockExchange.CoinBase;

internal sealed class CoinBaseTickerWire
{
    [JsonPropertyName("type")]
    public required string Type { get; init; }

    [JsonPropertyName("product_id")]
    public required string ProductId { get; init; }

    [JsonPropertyName("price")]
    public required string Price { get; init; }

    [JsonPropertyName("last_size")]
    public required string LastSize { get; init; }

    [JsonPropertyName("time")]
    public required string Time { get; init; }
}
