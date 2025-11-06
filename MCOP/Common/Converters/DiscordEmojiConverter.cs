using DSharpPlus.Commands.Converters;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Entities;

namespace MCOP.Common.Converters
{
    public class DiscordEmojiConverter : ISlashArgumentConverter<DiscordEmoji>, ITextArgumentConverter<DiscordEmoji>
    {
        public DiscordApplicationCommandOptionType ParameterType => DiscordApplicationCommandOptionType.String;
        public ConverterInputType RequiresText => ConverterInputType.Always;
        public string ReadableName => "Discord Emoji";

        public Task<Optional<DiscordEmoji>> ConvertAsync(ConverterContext context)
        {
            string? value = context.Argument?.ToString();

            if (string.IsNullOrWhiteSpace(value))
                return Task.FromResult(Optional.FromNoValue<DiscordEmoji>());

            if (context.Guild is not null)
            {
                var guildEmoji = context.Guild.Emojis.Where(x => x.Value.ToString() == value).ToList();
                if (guildEmoji.Count > 0)
                    Task.FromResult(Optional.FromValue(guildEmoji[0].Value));
            }

            DiscordEmoji.TryFromUnicode(context.Client, value, out DiscordEmoji? emoji);

            if (emoji is null)
                DiscordEmoji.TryFromName(context.Client, value, out emoji);

            return Task.FromResult(Optional.FromValue(emoji));
        }
    }
}
