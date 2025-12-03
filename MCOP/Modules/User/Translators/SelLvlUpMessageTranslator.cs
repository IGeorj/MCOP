using DSharpPlus.Commands.Processors.SlashCommands.Localization;

namespace MCOP.Modules.User.Translators
{
    public class SelLvlUpMessageTranslator : IInteractionLocalizer
    {
        public ValueTask<IReadOnlyDictionary<DiscordLocale, string>> TranslateAsync(string fullSymbolName) => fullSymbolName switch
        {
            "set_levelup_message.name" => ValueTask.FromResult<IReadOnlyDictionary<DiscordLocale, string>>(new Dictionary<DiscordLocale, string>
        {
            { DiscordLocale.en_US, "set_levelup_message" },
            { DiscordLocale.ru, "set_levelup_message" }
        }),
            "set_levelup_message.description" => ValueTask.FromResult<IReadOnlyDictionary<DiscordLocale, string>>(new Dictionary<DiscordLocale, string>
        {
            { DiscordLocale.en_US, "Sets the message template for the lvl up. Use {user}, {role}, {level}" },
            { DiscordLocale.ru, "Устанавливает шаблон сообщения при лвл апе. Используйте {user}, {role}, {level}" },
        }),
            _ => throw new KeyNotFoundException()
        };
    }
}
