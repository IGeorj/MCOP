using Newtonsoft.Json;

namespace MCOP.Core.Models
{
    public class DiscordPartialGuild
    {
        [JsonProperty("id")]
        public string Id { get; set; } = null!;

        [JsonProperty("name")]
        public string Name { get; set; } = null!;

        [JsonProperty("icon")]
        public string? Icon { get; set; }

        [JsonProperty("banner")]
        public string? Banner { get; set; }

        [JsonProperty("owner")]
        public bool Owner { get; set; }

        [JsonProperty("permissions")]
        public string Permissions { get; set; } = null!;

        [JsonProperty("features")]
        public string[] Features { get; set; } = null!;

        [JsonProperty("approximate_member_count")]
        public int ApproximateMemverCount { get; set; }

        [JsonProperty("approximate_presence_count")]
        public int ApproximatePresenceCount { get; set; }
    }
}
