using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using MCOP.Common;
using MCOP.Core.Services.Scoped;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace MCOP.EventListeners
{
    internal static partial class Listeners
    {
        private const string HeartEmojiName = ":heart:";

        public static async Task MessageReactionAddedEventHandler(DiscordClient client, MessageReactionAddedEventArgs e)
        {
            if (e.Guild is null || (e.Channel is null && e.Message.Channel is null) || e.Message is null
                || e.User.IsBot)
            {
                return;
            }

            if (e.Emoji.GetDiscordName() == HeartEmojiName)
            {
                await ChangeLikeAsync(e.Guild, e.Channel ?? e.Message.Channel, e.Message, e.Emoji, e.User, 1);
            }

            if (e.Guild.Emojis.TryGetValue(e.Emoji.Id, out _) && e.Emoji.GetDiscordName() != HeartEmojiName)
            {
               await ChangeEmojiRecievedCount(e.Guild, e.Channel ?? e.Message.Channel, e.Message, e.Emoji, e.User, 1);
            }
        }

        public static async Task MessageReactionRemovedEventHandler(DiscordClient client, MessageReactionRemovedEventArgs e)
        {
            if (e.Guild is null || (e.Channel is null && e.Message.Channel is null) || e.Message is null
                || e.User.IsBot)
            {
                return;
            }

            if (e.Emoji.GetDiscordName() == HeartEmojiName)
            {
                await ChangeLikeAsync(e.Guild, e.Channel ?? e.Message.Channel, e.Message, e.Emoji, e.User, -1);
            }

            if (e.Guild.Emojis.TryGetValue(e.Emoji.Id, out _) && e.Emoji.GetDiscordName() != HeartEmojiName)
            {
                await ChangeEmojiRecievedCount(e.Guild, e.Channel ?? e.Message.Channel, e.Message, e.Emoji, e.User, -1);
            }
        }

        private static async Task ChangeLikeAsync(DiscordGuild? guild, DiscordChannel? channel, DiscordMessage msg, DiscordEmoji emoji, DiscordUser user, int count)
        {
            if (msg.Author is null && channel is not null)
            {
                msg = await channel.GetMessageAsync(msg.Id);
            }

            if (emoji.GetDiscordName() == HeartEmojiName && !(msg?.Author?.Id == user.Id))
            {
                UserStatsService statsService = Services.GetRequiredService<UserStatsService>();
                await statsService.ChangeLikeAsync(guild.Id, msg.Author.Id, msg.Id, count);
                Log.Information("User {Username} heart emoji {count}. MessageId:{Id} AuthorId:{authorId}", user.Username, count, msg.Id, msg.Author.Id);

            }
        }

        private static async Task ChangeEmojiRecievedCount(DiscordGuild? guild, DiscordChannel? channel, DiscordMessage msg, DiscordEmoji emoji, DiscordUser user, int count)
        {
            if (msg.Author is null && channel is not null)
            {
                msg = await channel.GetMessageAsync(msg.Id);
            }

            if (!(msg?.Author?.Id == user.Id))
            {
                GuildEmojiService guildEmojiService = Services.GetRequiredService<GuildEmojiService>();
                await guildEmojiService.ChangeUserRecievedEmojiCountAsync(emoji, guild.Id, msg.Author.Id, count);
                var emojiName = emoji.Name;
                Log.Information("User {Username} emoji {emojiName} {count}. MessageId:{Id} AuthorId:{authorId}", user.Username, emojiName, count, msg.Id, msg.Author.Id);
            }
        }
    }
}
