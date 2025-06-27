using System.Text.Json.Serialization;

namespace StockInfoApp.Models
{
    public class RecommendationTrend
    {
        [JsonPropertyName("symbol")]
        public string Symbol { get; set; }

        [JsonPropertyName("period")]
        public string Period { get; set; }

        [JsonPropertyName("strongBuy")]
        public int StrongBuy { get; set; }

        [JsonPropertyName("buy")]
        public int Buy { get; set; }

        [JsonPropertyName("hold")]
        public int Hold { get; set; }

        [JsonPropertyName("sell")]
        public int Sell { get; set; }

        [JsonPropertyName("strongSell")]
        public int StrongSell { get; set; }
    }
}
