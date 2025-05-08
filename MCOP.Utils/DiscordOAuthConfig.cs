using Newtonsoft.Json;

namespace MCOP.Utils;

public class DiscordOAuthConfig
{
    [JsonProperty("clientId")]
    public string ClientId { get; set; } = "YOUR_DISCORD_CLIENT_ID";

    [JsonProperty("clientSecret")]
    public string ClientSecret { get; set; } = "YOUR_DISCORD_CLIENT_SECRET";

    [JsonProperty("redirectUri")]
    public string RedirectUri { get; set; } = "http://localhost:5173/oauth/callback";
}
