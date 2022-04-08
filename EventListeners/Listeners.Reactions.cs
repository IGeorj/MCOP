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

            DiscordMessage msg = e.Message;

            if (msg.Author is null)
            {
                msg = await e.Channel.GetMessageAsync(msg.Id);
            }

            if (e.Emoji.GetDiscordName() == ":heart:" && !(msg.Author.Id == e.User.Id))
            {
                UserStatsService statsService = bot.Services.GetRequiredService<UserStatsService>();
                await statsService.ChangeLikeAsync(e.Guild.Id, msg.Author.Id, 1);
                Log.Information("User {Username} ADD heart emoji. MessageId:{Id} AuthorId:{authorId}", e.User.Username, msg.Id, msg.Author.Id);
            }
        }

        [AsyncEventListener(DiscordEventType.MessageReactionRemoved)]
        public static async Task MessageReactionRemovedEventHandlerAsync(Bot bot, MessageReactionRemoveEventArgs e)
        {
            if (e.Guild is null || e.Channel is null || e.Message is null
                || e.User.IsBot || e.Emoji.GetDiscordName() != ":heart:")
            {
                return;
            }

            DiscordMessage msg = e.Message;

            if (msg.Author is null)
            {
                msg = await e.Channel.GetMessageAsync(msg.Id);
            }

            if (e.Emoji.GetDiscordName() == ":heart:" && !(msg.Author.Id == e.User.Id))
            {
                UserStatsService statsService = bot.Services.GetRequiredService<UserStatsService>();
                await statsService.ChangeLikeAsync(e.Guild.Id, msg.Author.Id, -1);
                Log.Information("User {Username} REMOVE heart emoji. MessageId:{Id} AuthorId:{authorId}", e.User.Username, msg.Id, msg.Author.Id);

            }
        }
    }
}
