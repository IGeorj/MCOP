using DSharpPlus.Commands.Processors.SlashCommands.Localization;

namespace MCOP.Modules.User.Translators
{
    public class ReactionSettingsTranslator : IInteractionLocalizer
    {
        public ValueTask<IReadOnlyDictionary<DiscordLocale, string>> TranslateAsync(string fullSymbolName) => fullSymbolName switch
        {
            "settings.name" => ValueTask.FromResult<IReadOnlyDictionary<DiscordLocale, string>>(new Dictionary<DiscordLocale, string>
        {
            { DiscordLocale.en_US, "settings" },
            { DiscordLocale.ru, "settings" }
        }),
            "settings.description" => ValueTask.FromResult<IReadOnlyDictionary<DiscordLocale, string>>(new Dictionary<DiscordLocale, string>
        {
            { DiscordLocale.en_US, "Reaction settings" },
            { DiscordLocale.ru, "Настройка реакций" },
        }),
            _ => throw new KeyNotFoundException()
        };
    }
}
