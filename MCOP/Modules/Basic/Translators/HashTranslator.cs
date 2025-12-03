using DSharpPlus.Commands.Processors.SlashCommands.Localization;

namespace MCOP.Modules.Basic.Translators
{
    public class HashTranslator : IInteractionLocalizer
    {
        public ValueTask<IReadOnlyDictionary<DiscordLocale, string>> TranslateAsync(string fullSymbolName) => fullSymbolName switch
        {
            "hash.name" => ValueTask.FromResult<IReadOnlyDictionary<DiscordLocale, string>>(new Dictionary<DiscordLocale, string>
        {
            { DiscordLocale.en_US, "hash" },
            { DiscordLocale.ru, "hash" }
        }),
            "hash.description" => ValueTask.FromResult<IReadOnlyDictionary<DiscordLocale, string>>(new Dictionary<DiscordLocale, string>
        {
            { DiscordLocale.en_US, "Hashes images from a message" },
            { DiscordLocale.ru, "Хеширует изображения из сообщения" },
        }),
            _ => throw new KeyNotFoundException()
        };
    }
}
