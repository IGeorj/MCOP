using Newtonsoft.Json;

namespace MCOP.Core.Services.Scoped.OAuth
{
    public class DiscordTokenResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; } = null!;

        [JsonProperty("token_type")]
        public string TokenType { get; set; } = null!;

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonProperty("expires_at")]
        public DateTime ExpiresAt { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; } = null!;

        [JsonProperty("scope")]
        public string Scope { get; set; } = null!;
    }
}
