using Newtonsoft.Json;

namespace MCOP.Utils;

public sealed class JwtConfig
{
    [JsonProperty("key")]
    public string Key { get; set; } = "SOME_SUPER_SECRET_KEY_CHANGE_THIS";

    [JsonProperty("issuer")]
    public string Issuer { get; set; } = "MCOP";

    [JsonProperty("audience")]
    public string Audience { get; set; } = "MCOP";
}
