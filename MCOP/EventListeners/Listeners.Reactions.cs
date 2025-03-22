using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
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

            await ChangeLikeAsync(e.Guild, e.Channel ?? e.Message.Channel, e.Message, e.Emoji, e.User, 1);


            if (!e.Emoji.IsManaged && e.Emoji.GetDiscordName() != HeartEmojiName && e.Guild.Emojis.TryGetValue(e.Emoji.Id, out _))
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

            await ChangeLikeAsync(e.Guild, e.Channel ?? e.Message.Channel, e.Message, e.Emoji, e.User, -1);

            if (!e.Emoji.IsManaged && e.Emoji.GetDiscordName() != HeartEmojiName && e.Guild.Emojis.TryGetValue(e.Emoji.Id, out _))
            {
                await ChangeEmojiRecievedCount(e.Guild, e.Channel ?? e.Message.Channel, e.Message, e.Emoji, e.User, -1);
            }
        }

        private static async Task ChangeLikeAsync(DiscordGuild? guild, DiscordChannel? channel, DiscordMessage msg, DiscordEmoji emoji, DiscordUser user, int count)
        {
            if (emoji.GetDiscordName() != HeartEmojiName || guild is null) return;

            if (msg.Author is null && channel is not null)
            {
                msg = await channel.GetMessageAsync(msg.Id);
            }

            if (!(msg?.Author?.Id == user.Id))
            {
                Log.Information("User {Username} change {emoji} {count}", user.Username, emoji.GetDiscordName(), count);

                LikeService likeService = Services.GetRequiredService<LikeService>();

                if (count > 0)
                {
                    await likeService.AddLikeAsync(guild.Id, msg.Author.Id, msg.Id);
                }
                else
                {
                    await likeService.RemoveLikeAsync(guild.Id, msg.Author.Id, msg.Id);
                }
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
                Log.Information("User {Username} change {emoji} {count}", user.Username, emoji.GetDiscordName(), count);

                GuildUserEmojiService guildEmojiService = Services.GetRequiredService<GuildUserEmojiService>();

                if (count > 0) {
                    await guildEmojiService.AddRecievedAmountAsync(guild.Id, msg.Author.Id, emoji.Id, count);
                }
                else
                {
                    await guildEmojiService.RemoveRecievedAmountAsync(guild.Id, msg.Author.Id, emoji.Id, count * -1);
                }
            }
        }
    }
}
