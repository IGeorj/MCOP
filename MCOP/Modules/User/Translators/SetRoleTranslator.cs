using DSharpPlus.Commands.Processors.SlashCommands.Localization;

namespace MCOP.Modules.User.Translators
{
    public class SetRoleTranslator : IInteractionLocalizer
    {
        public ValueTask<IReadOnlyDictionary<DiscordLocale, string>> TranslateAsync(string fullSymbolName) => fullSymbolName switch
        {
            "set_role.name" => ValueTask.FromResult<IReadOnlyDictionary<DiscordLocale, string>>(new Dictionary<DiscordLocale, string>
        {
            { DiscordLocale.en_US, "set_role" },
            { DiscordLocale.ru, "set_role" }
        }),
            "set_role.description" => ValueTask.FromResult<IReadOnlyDictionary<DiscordLocale, string>>(new Dictionary<DiscordLocale, string>
        {
            { DiscordLocale.en_US, "Sets level for the role" },
            { DiscordLocale.ru, "Устанавливает уровень для роли" },
        }),
            _ => throw new KeyNotFoundException()
        };
    }
}
