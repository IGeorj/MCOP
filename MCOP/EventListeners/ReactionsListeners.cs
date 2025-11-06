using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using MCOP.Core.Models;
using MCOP.Core.Services.Scoped;
using Serilog;

namespace MCOP.EventListeners
{
    public sealed class ReactionsListeners
    {
        private readonly IReactionService _reactionService;
        private readonly IGuildConfigService guildConfigService;

        public ReactionsListeners(IReactionService reactionService, IGuildConfigService guildConfigService)
        {
            _reactionService = reactionService;
            this.guildConfigService = guildConfigService;
        }

        public async Task MessageReactionAddedEventHandler(DiscordClient client, MessageReactionAddedEventArgs e)
        {
            if (e.Guild is null || (e.Channel is null && e.Message?.Channel is null) || e.Message is null || e.User.IsBot)
                return;

            var channel = e.Channel ?? e.Message.Channel;
            var guildId = e.Guild.Id;

            var config = await guildConfigService.GetOrAddGuildConfigAsync(guildId);
            if (!config.ReactionTrackingEnabled)
                return;

            await HandleReactionAsync(e.Guild, channel!, e.Message, e.Emoji, e.User, 1, config);
        }

        public async Task MessageReactionRemovedEventHandler(DiscordClient client, MessageReactionRemovedEventArgs e)
        {
            if (e.Guild is null || (e.Channel is null && e.Message?.Channel is null) || e.Message is null || e.User.IsBot)
                return;

            var channel = e.Channel ?? e.Message.Channel;
            var guildId = e.Guild.Id;

            var config = await guildConfigService.GetOrAddGuildConfigAsync(guildId);
            if (!config.ReactionTrackingEnabled)
                return;

            await HandleReactionAsync(e.Guild, channel!, e.Message, e.Emoji, e.User, -1, config);
        }

        private async Task HandleReactionAsync(
            DiscordGuild guild,
            DiscordChannel channel,
            DiscordMessage msg,
            DiscordEmoji emoji,
            DiscordUser user,
            int delta,
            GuildConfigDto config)
        {
            string emojiName = emoji.Name;
            ulong emojiId = emoji.Id;

            if (msg.Author is null)
                msg = await channel.GetMessageAsync(msg.Id);

            if (msg?.Author is null || msg.Author.Id == user.Id)
                return;

            if (emojiId != 0 && guild.Emojis.ContainsKey(emojiId))
            {
                Log.Information("User {Username} changed custom reaction {emoji} by {delta}", user.Username, emojiName, delta);

                if (delta > 0)
                    await _reactionService.AddCustomReactionAsync(guild.Id, channel.Id, msg.Id, user.Id, msg.Author.Id, emojiName, emojiId);
                else
                    await _reactionService.RemoveReactionAsync(guild.Id, channel.Id, msg.Id, user.Id, emojiName, emojiId);
            }
            else if(emojiId == 0 && !emoji.IsManaged)
            {
                Log.Information("User {Username} changed unicode reaction {emoji} by {delta}", user.Username, emojiName, delta);

                if (delta > 0)
                    await _reactionService.AddUnicodeReactionAsync(guild.Id, channel.Id, msg.Id, user.Id, msg.Author.Id, emojiName);
                else
                    await _reactionService.RemoveReactionAsync(guild.Id, channel.Id, msg.Id, user.Id, emojiName, emojiId);
            }
        }
    }
}