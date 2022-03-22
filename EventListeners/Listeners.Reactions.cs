using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using MCOP.EventListeners.Attributes;
using MCOP.EventListeners.Common;
using MCOP.Modules.Basic.Services;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace MCOP.EventListeners
{
    internal static partial class Listeners
    {

        [AsyncEventListener(DiscordEventType.MessageReactionAdded)]
        public static async Task MessageReactionAddedEventHandlerAsync(Bot bot, MessageReactionAddEventArgs e)
        {
            if (e.Guild is null || e.Channel is null || e.Message is null 
                || e.User.IsBot || e.Emoji.GetDiscordName() != ":heart:")
            {
                return;
            }

            ulong authorId;
            DiscordMessage msg;

            if (e.Message.Author is null)
            {
                msg = await e.Channel.GetMessageAsync(e.Message.Id);
                authorId = msg.Author.Id;
            }
            else
            {
                msg = e.Message;
                authorId = msg.Author.Id;
            }

            if (e.Emoji.GetDiscordName() == ":heart:" && !(authorId == e.User.Id))
            {
                UserStatsService statsService = bot.Services.GetRequiredService<UserStatsService>();
                await statsService.ChangeLikeAsync(e.Guild.Id, authorId, 1);
                Log.Information("User {Username} ADD heart emoji. MessageId:{Id} AuthorId:{authorId}", e.User.Username, msg.Id, authorId);
            }

            return;
        }

        [AsyncEventListener(DiscordEventType.MessageReactionRemoved)]
        public static async Task MessageReactionRemovedEventHandlerAsync(Bot bot, MessageReactionRemoveEventArgs e)
        {
            if (e.Guild is null || e.Channel is null || e.Message is null
                || e.User.IsBot || e.Emoji.GetDiscordName() != ":heart:")
            {
                return;
            }

            ulong authorId;
            DiscordMessage msg;

            if (e.Message.Author is null)
            {
                msg = await e.Channel.GetMessageAsync(e.Message.Id);
                authorId = msg.Author.Id;
            }
            else
            {
                msg = e.Message;
                authorId = msg.Author.Id;
            }

            if (e.Emoji.GetDiscordName() == ":heart:" && !(authorId == e.User.Id))
            {
                UserStatsService statsService = bot.Services.GetRequiredService<UserStatsService>();
                await statsService.ChangeLikeAsync(e.Guild.Id, authorId, -1);
                Log.Information("User {Username} REMOVE heart emoji. MessageId:{Id} AuthorId:{authorId}", e.User.Username, msg.Id, authorId);

            }

            return;
        }
    }
}
