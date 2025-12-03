using DSharpPlus.Commands.Processors.SlashCommands.Localization;

namespace MCOP.Modules.User.Translators
{
    public class StatsTopTranslator : IInteractionLocalizer
    {
        public ValueTask<IReadOnlyDictionary<DiscordLocale, string>> TranslateAsync(string fullSymbolName) => fullSymbolName switch
        {
            "top.name" => ValueTask.FromResult<IReadOnlyDictionary<DiscordLocale, string>>(new Dictionary<DiscordLocale, string>
        {
            { DiscordLocale.en_US, "top" },
            { DiscordLocale.ru, "top" }
        }),
            "top.description" => ValueTask.FromResult<IReadOnlyDictionary<DiscordLocale, string>>(new Dictionary<DiscordLocale, string>
        {
            { DiscordLocale.en_US, "Shows the top users of the server" },
            { DiscordLocale.ru, "Показывает топ пользователей сервера" },
        }),
            _ => throw new KeyNotFoundException()
        };
    }
}
