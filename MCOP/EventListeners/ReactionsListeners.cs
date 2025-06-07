using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using MCOP.Core.Services.Scoped;
using Serilog;

namespace MCOP.EventListeners
{
    public class ReactionsListeners
    {
        private const string HeartEmojiName = ":heart:";

        private readonly IGuildUserEmojiService _userEmojiService;
        private readonly ILikeService _likeService;

        public ReactionsListeners(IGuildUserEmojiService userEmojiService, ILikeService likeService)
        {
            _userEmojiService = userEmojiService;
            _likeService = likeService;
        }

        public async Task MessageReactionAddedEventHandler(DiscordClient client, MessageReactionAddedEventArgs e)
        {
            var isChannelNull = e.Channel is null && e.Message.Channel is null;

            if (e.Guild is null || isChannelNull || e.Message is null || e.User.IsBot)
                return;

            await ChangeLikeAsync(client, e.Guild, e.Channel ?? e.Message.Channel, e.Message, e.Emoji, e.User, 1);


            if (!e.Emoji.IsManaged && e.Emoji.GetDiscordName() != HeartEmojiName && e.Guild.Emojis.TryGetValue(e.Emoji.Id, out _))
               await ChangeEmojiRecievedCount(client, e.Guild, e.Channel ?? e.Message.Channel, e.Message, e.Emoji, e.User, 1);
        }

        public async Task MessageReactionRemovedEventHandler(DiscordClient client, MessageReactionRemovedEventArgs e)
        {
            var isChannelNull = e.Channel is null && e.Message.Channel is null;

            if (e.Guild is null || isChannelNull || e.Message is null || e.User.IsBot)
                return;

            await ChangeLikeAsync(client, e.Guild, e.Channel ?? e.Message.Channel, e.Message, e.Emoji, e.User, -1);

            if (!e.Emoji.IsManaged && e.Emoji.GetDiscordName() != HeartEmojiName && e.Guild.Emojis.TryGetValue(e.Emoji.Id, out _))
                await ChangeEmojiRecievedCount(client, e.Guild, e.Channel ?? e.Message.Channel, e.Message, e.Emoji, e.User, -1);
        }

        private async Task ChangeLikeAsync(DiscordClient client, DiscordGuild guild, DiscordChannel? channel, DiscordMessage msg, DiscordEmoji emoji, DiscordUser user, int count)
        {
            if (emoji.GetDiscordName() != HeartEmojiName) return;

            if (msg.Author is null && channel is not null)
                msg = await channel.GetMessageAsync(msg.Id);

            if (msg?.Author is null || msg.Author.Id == user.Id)
                return;

            Log.Information("User {Username} change {emoji} {count}", user.Username, emoji.GetDiscordName(), count);

            if (count > 0)
                await _likeService.AddLikeAsync(guild.Id, msg.Author.Id, msg.Id);
            else
                await _likeService.RemoveLikeAsync(guild.Id, msg.Author.Id, msg.Id);
        }

        private async Task ChangeEmojiRecievedCount(DiscordClient client, DiscordGuild guild, DiscordChannel? channel, DiscordMessage msg, DiscordEmoji emoji, DiscordUser user, int count)
        {
            if (msg.Author is null && channel is not null)
                msg = await channel.GetMessageAsync(msg.Id);

            if (msg?.Author is null || msg.Author.Id == user.Id)
                return;

            Log.Information("User {Username} change {emoji} {count}", user.Username, emoji.GetDiscordName(), count);

            if (count > 0)
                await _userEmojiService.AddRecievedAmountAsync(guild.Id, msg.Author.Id, emoji.Id, count);
            else
                await _userEmojiService.RemoveRecievedAmountAsync(guild.Id, msg.Author.Id, emoji.Id, count * -1);
        }
    }
}
