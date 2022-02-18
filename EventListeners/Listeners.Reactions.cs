using DSharpPlus.EventArgs;
using MCOP.EventListeners.Attributes;
using MCOP.EventListeners.Common;
using MCOP.Modules.Basic.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCOP.EventListeners
{
    internal static partial class Listeners
    {

        [AsyncEventListener(DiscordEventType.MessageReactionAdded)]
        public static async Task MessageReactionAddedEventHandlerAsync(Bot bot, MessageReactionAddEventArgs e)
        {
            if (e.Guild is null || e.Channel is null || e.Message is null || e.User.IsBot)
            {
                return;
            }

            ulong authorId;

            if (e.Message.Author is null)
            {
                var channel = await bot.Client.GetChannelAsync(e.Channel.Id);
                var msg = await channel.GetMessageAsync(e.Message.Id);
                authorId = msg.Author.Id;
            }
            else
            {
                authorId = e.Message.Author.Id;
            }

            if (e.Channel.Id == nsfwAnimeChannelId && e.Emoji.GetDiscordName() == ":heart:" && !(authorId == e.User.Id))
            {
                UserStatsService statsService = bot.Services.GetRequiredService<UserStatsService>();
                await statsService.ChangeLikeAsync(e.Guild.Id, authorId, 1);
            }

            return;
        }

        [AsyncEventListener(DiscordEventType.MessageReactionRemoved)]
        public static async Task MessageReactionRemovedEventHandlerAsync(Bot bot, MessageReactionRemoveEventArgs e)
        {
            if (e.Guild is null || e.Channel is null || e.Message is null || e.User.IsBot)
            {
                return;

            }

            ulong authorId;

            if (e.Message.Author is null)
            {
                var channel = await bot.Client.GetChannelAsync(e.Channel.Id);
                var msg = await channel.GetMessageAsync(e.Message.Id);
                authorId = msg.Author.Id;
            }
            else
            {
                authorId = e.Message.Author.Id;
            }

            if (e.Channel.Id == nsfwAnimeChannelId && e.Emoji.GetDiscordName() == ":heart:" && !(authorId == e.User.Id))
            {
                UserStatsService statsService = bot.Services.GetRequiredService<UserStatsService>();
                await statsService.ChangeLikeAsync(e.Guild.Id, authorId, -1);
            }

            return;
        }
    }
}
