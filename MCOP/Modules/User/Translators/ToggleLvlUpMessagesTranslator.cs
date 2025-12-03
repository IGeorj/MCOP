using DSharpPlus.Commands.Processors.SlashCommands.Localization;

namespace MCOP.Modules.User.Translators
{
    public class ToggleLvlUpMessagesTranslator : IInteractionLocalizer
    {
        public ValueTask<IReadOnlyDictionary<DiscordLocale, string>> TranslateAsync(string fullSymbolName) => fullSymbolName switch
        {
            "toggle_levelup_messages.name" => ValueTask.FromResult<IReadOnlyDictionary<DiscordLocale, string>>(new Dictionary<DiscordLocale, string>
        {
            { DiscordLocale.en_US, "toggle_levelup_messages" },
            { DiscordLocale.ru, "toggle_levelup_messages" }
        }),
            "toggle_levelup_messages.description" => ValueTask.FromResult<IReadOnlyDictionary<DiscordLocale, string>>(new Dictionary<DiscordLocale, string>
        {
            { DiscordLocale.en_US, "Enables/disables sending lvl up messages" },
            { DiscordLocale.ru, "Включает/выключает отправку сообщений о повышении уровня" },
        }),
            _ => throw new KeyNotFoundException()
        };
    }
}
