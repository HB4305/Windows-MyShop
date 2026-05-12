using System.Text.Json.Serialization;

namespace MyShop.Models.Ai
{
    public class AiSearchFilter
    {
        [JsonPropertyName("keyword")]
        public string Keyword { get; set; } = string.Empty;

        [JsonPropertyName("min_price")]
        public double? MinPrice { get; set; }

        [JsonPropertyName("max_price")]
        public double? MaxPrice { get; set; }

        [JsonPropertyName("reasoning")]
        public string Reason { get; set; } = string.Empty;
    }
}
