using DSharpPlus.Commands.Processors.SlashCommands.Localization;

namespace MCOP.Modules.User.Translators
{
    public class StatsTranslator : IInteractionLocalizer
    {
        public ValueTask<IReadOnlyDictionary<DiscordLocale, string>> TranslateAsync(string fullSymbolName) => fullSymbolName switch
        {
            "stats.name" => ValueTask.FromResult<IReadOnlyDictionary<DiscordLocale, string>>(new Dictionary<DiscordLocale, string>
        {
            { DiscordLocale.en_US, "stats" },
            { DiscordLocale.ru, "stats" }
        }),
            "stats.description" => ValueTask.FromResult<IReadOnlyDictionary<DiscordLocale, string>>(new Dictionary<DiscordLocale, string>
        {
            { DiscordLocale.en_US, "Shows user statistics" },
            { DiscordLocale.ru, "Показывает статистику пользователя" },
        }),
            _ => throw new KeyNotFoundException()
        };
    }
}
