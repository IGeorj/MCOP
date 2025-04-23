using System.Text.Json.Serialization;

namespace MCOP.Core.Services.Scoped.AI
{
    public class OpenRouterKeyInfo
    {
        [JsonPropertyName("data")]
        public KeyData Data { get; set; } = null!;
    }

    public class KeyData
    {
        [JsonPropertyName("label")]
        public string Label { get; set; } = null!;

        [JsonPropertyName("usage")]
        public decimal Usage { get; set; }

        [JsonPropertyName("limit")]
        public decimal? Limit { get; set; }

        [JsonPropertyName("is_free_tier")]
        public bool IsFreeTier { get; set; }

        [JsonPropertyName("rate_limit")]
        public RateLimit RateLimit { get; set; } = null!;
    }

    public class RateLimit
    {
        [JsonPropertyName("requests")]
        public int Requests { get; set; }

        [JsonPropertyName("interval")]
        public string Interval { get; set; } = null!;
    }
}
