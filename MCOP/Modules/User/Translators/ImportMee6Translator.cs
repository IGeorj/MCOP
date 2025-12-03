using DSharpPlus.Commands.Processors.SlashCommands.Localization;

namespace MCOP.Modules.User.Translators
{
    public class ImportMee6Translator : IInteractionLocalizer
    {
        public ValueTask<IReadOnlyDictionary<DiscordLocale, string>> TranslateAsync(string fullSymbolName) => fullSymbolName switch
        {
            "import_mee6.name" => ValueTask.FromResult<IReadOnlyDictionary<DiscordLocale, string>>(new Dictionary<DiscordLocale, string>
        {
            { DiscordLocale.en_US, "import_mee6" },
            { DiscordLocale.ru, "import_mee6" }
        }),
            "import_mee6.description" => ValueTask.FromResult<IReadOnlyDictionary<DiscordLocale, string>>(new Dictionary<DiscordLocale, string>
        {
            { DiscordLocale.en_US, "Imports levels from mee6" },
            { DiscordLocale.ru, "Импортирует уровни из mee6" },
        }),
            _ => throw new KeyNotFoundException()
        };
    }
}
