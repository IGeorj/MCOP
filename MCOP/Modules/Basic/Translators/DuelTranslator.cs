using DSharpPlus.Commands.Processors.SlashCommands.Localization;

namespace MCOP.Modules.Basic.Translators
{
    public class DuelTranslator : IInteractionLocalizer
    {
        public ValueTask<IReadOnlyDictionary<DiscordLocale, string>> TranslateAsync(string fullSymbolName) => fullSymbolName switch
        {
            "duel.name" => ValueTask.FromResult<IReadOnlyDictionary<DiscordLocale, string>>(new Dictionary<DiscordLocale, string>
        {
            { DiscordLocale.en_US, "duel" },
            { DiscordLocale.ru, "duel" }
        }),
            "duel.description" => ValueTask.FromResult<IReadOnlyDictionary<DiscordLocale, string>>(new Dictionary<DiscordLocale, string>
        {
            { DiscordLocale.en_US, "Creates a timeout duel" },
            { DiscordLocale.ru, "Создаёт дуэль за таймаут" },
        }),
            _ => throw new KeyNotFoundException()
        };
    }
}
