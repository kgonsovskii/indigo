using System.Text.Json.Serialization;

namespace Feed.Parser.CoinBase;

internal sealed class CoinbaseMsg
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("product_id")]
    public string? ProductId { get; set; }

    [JsonPropertyName("price")]
    public string? Price { get; set; }

    [JsonPropertyName("last_size")]
    public string? LastSize { get; set; }

    [JsonPropertyName("time")]
    public string? Time { get; set; }
}
