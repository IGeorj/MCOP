using MCOP.Core.Services.Shared.Common;
using MCOP.Utils.Interfaces;
using Newtonsoft.Json;
using System.Text;

namespace MCOP.Core.Services.Shared;

public sealed class ConfigurationService : ISharedService
{
    public BotConfiguration CurrentConfiguration { get; private set; } = new BotConfiguration();


    public async Task<BotConfiguration> LoadConfigAsync(string path = "resources/config.json")
    {
        string json = "{}";
        var utf8 = new UTF8Encoding(false);
        var fi = new FileInfo(path);
        if (!fi.Exists)
        {
            Console.WriteLine("Loading configuration failed!");

            Directory.CreateDirectory("resources");

            json = JsonConvert.SerializeObject(new BotConfiguration(), Formatting.Indented);
            using (FileStream fs = fi.Create())
            using (var sw = new StreamWriter(stream: fs, utf8))
            {
                await sw.WriteAsync(json);
                await sw.FlushAsync();
            }

            Console.WriteLine("New default configuration file has been created at:");
            Console.WriteLine(fi.FullName);
            Console.WriteLine("Please fill it with appropriate values and re-run the bot.");

            throw new IOException("Configuration file not found!");
        }

        using (FileStream fs = fi.OpenRead())
        using (var sr = new StreamReader(fs, utf8))
            json = await sr.ReadToEndAsync();

        CurrentConfiguration = JsonConvert.DeserializeObject<BotConfiguration>(json) ?? throw new JsonSerializationException();
        return CurrentConfiguration;
    }
}
