using Newtonsoft.Json;

namespace MCOP.Utils;

public sealed class DBConfig
{
    [JsonProperty("database")]
    public string DatabaseName { get; set; } = "mcop_db";

    [JsonProperty("provider")]
    public DBProvider Provider { get; set; } = DBProvider.Sqlite;

    [JsonProperty("hostname")]
    public string Hostname { get; set; } = "localhost";

    [JsonProperty("password")]
    public string Password { get; set; } = "password";

    [JsonProperty("port")]
    public int Port { get; set; } = 5432;

    [JsonProperty("username")]
    public string Username { get; set; } = "username";
}
