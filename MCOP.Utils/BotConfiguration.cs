using Newtonsoft.Json;
using Serilog;
using Serilog.Events;

namespace MCOP.Utils;

public sealed class BotConfiguration
{
    public const string DefaultPrefix = "!m";


    [JsonProperty("db-config")]
    public DBConfig DatabaseConfig { get; set; } = new DBConfig();

    [JsonProperty("discord-oauth")]
    public DiscordOAuthConfig DiscordOAuthConfig { get; set; } = new DiscordOAuthConfig();

    [JsonProperty("jwt-config")]
    public JwtConfig JwtConfig { get; set; } = new JwtConfig();

    [JsonProperty("token")]
    public string Token { get; set; } = string.Empty;

    [JsonProperty("prefix")]
    public string Prefix { get; set; } = DefaultPrefix;

    [JsonProperty("log-level")]
    public LogEventLevel LogLevel { get; set; } = LogEventLevel.Information;

    [JsonProperty("log-path")]
    public string LogPath { get; set; } = "bot.log";

    [JsonProperty("log-file-rolling")]
    public RollingInterval RollingInterval { get; set; } = RollingInterval.Day;

    [JsonProperty("log-to-file")]
    public bool LogToFile { get; set; } = false;

    [JsonProperty("log-buffer")]
    public bool UseBufferedFileLogger { get; set; } = false;

    [JsonProperty("log-max-files")]
    public int? MaxLogFiles { get; set; }

    [JsonProperty("log-template")]
    public string? CustomLogTemplate { get; set; }

    [JsonProperty("sankaku-password")]
    public string? SankakuPassword { get; set; }

    [JsonProperty("sankaku-restricted-tags")]
    public string? SankakuRestrictegTags { get; set; }

    [JsonProperty("e621-hash-password")]
    public string? E621HashPassword { get; set; }

    [JsonProperty("e621-restricted-tags")]
    public string? E621RestrictegTags { get; set; }

    [JsonProperty("gelbooru-restricted-tags")]
    public string? GelbooruRestrictegTags { get; set; }

    [JsonProperty("ai-api-key")]
    public string? AIApiKey { get; set; }

    [JsonProperty("openrounter-api-key")]
    public string? OpenRounterApiKey { get; set; }

}
