using MCOP.Utils.Interfaces;
using Newtonsoft.Json;
using System.Text;

namespace MCOP.Utils;

public sealed class ConfigurationService : ISharedService
{
    private const string ConfigFilePath = "resources/config.json";

    public BotConfiguration CurrentConfiguration { get; private set; } = new();

    public ConfigurationService()
    {
    }

    public async Task<BotConfiguration> LoadConfigAsync()
    {
        if (string.IsNullOrEmpty(CurrentConfiguration.Token))
        {
            CurrentConfiguration = await LoadFromJsonFileAsync();
        }

        if (string.IsNullOrEmpty(CurrentConfiguration.Token))
            throw new Exception("Discord bot token is not configured!");

        return CurrentConfiguration;
    }

    private async Task<BotConfiguration> LoadFromJsonFileAsync()
    {
        var utf8 = new UTF8Encoding(false);
        var fi = new FileInfo(ConfigFilePath);

        if (!fi.Exists)
        {
            await CreateDefaultConfigFileAsync(fi, utf8);
        }

        string json;
        using (var fs = fi.OpenRead())
        using (var sr = new StreamReader(fs, utf8))
        {
            json = await sr.ReadToEndAsync();
        }

        var config = JsonConvert.DeserializeObject<BotConfiguration>(json) ?? throw new JsonSerializationException();
        return config;
    }

    private async Task CreateDefaultConfigFileAsync(FileInfo fi, Encoding encoding)
    {
        Directory.CreateDirectory("resources");

        var defaultConfig = new BotConfiguration();

        var json = JsonConvert.SerializeObject(defaultConfig, Formatting.Indented);

        await using var fs = fi.Create();
        await using var sw = new StreamWriter(fs, encoding);
        await sw.WriteAsync(json);
        await sw.FlushAsync();
    }
}