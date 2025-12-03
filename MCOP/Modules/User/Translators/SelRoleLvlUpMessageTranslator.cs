using DSharpPlus.Commands.Processors.SlashCommands.Localization;

namespace MCOP.Modules.User.Translators
{
    public class SelRoleLvlUpMessageTranslator : IInteractionLocalizer
    {
        public ValueTask<IReadOnlyDictionary<DiscordLocale, string>> TranslateAsync(string fullSymbolName) => fullSymbolName switch
        {
            "set_role_levelup_message.name" => ValueTask.FromResult<IReadOnlyDictionary<DiscordLocale, string>>(new Dictionary<DiscordLocale, string>
        {
            { DiscordLocale.en_US, "set_role_levelup_message" },
            { DiscordLocale.ru, "set_role_levelup_message" }
        }),
            "set_role_levelup_message.description" => ValueTask.FromResult<IReadOnlyDictionary<DiscordLocale, string>>(new Dictionary<DiscordLocale, string>
        {
            { DiscordLocale.en_US, "Sets the message template for the lvl up. Use {user}, {role}, {level}" },
            { DiscordLocale.ru, "Устанавливает индивидуальный шаблон сообщения для роли. Используйте {user}, {role}, {level}" },
        }),
            _ => throw new KeyNotFoundException()
        };
    }
}
