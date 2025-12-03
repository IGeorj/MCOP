using DSharpPlus.Commands.Processors.SlashCommands.Localization;

namespace MCOP.Modules.User.Translators
{
    public class SetBlockedRoleTranslator : IInteractionLocalizer
    {
        public ValueTask<IReadOnlyDictionary<DiscordLocale, string>> TranslateAsync(string fullSymbolName) => fullSymbolName switch
        {
            "set_blocked_role.name" => ValueTask.FromResult<IReadOnlyDictionary<DiscordLocale, string>>(new Dictionary<DiscordLocale, string>
        {
            { DiscordLocale.en_US, "set_blocked_role" },
            { DiscordLocale.ru, "set_blocked_role" }
        }),
            "set_blocked_role.description" => ValueTask.FromResult<IReadOnlyDictionary<DiscordLocale, string>>(new Dictionary<DiscordLocale, string>
        {
            { DiscordLocale.en_US, "Sets a block on gaining experience for the role" },
            { DiscordLocale.ru, "Устанавливает блокировку получения опыта для роли" },
        }),
            _ => throw new KeyNotFoundException()
        };
    }
}
