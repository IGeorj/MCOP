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
        public static async Task MessageReactionAddedEventHandler(DiscordClient client, MessageReactionAddedEventArgs e)
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

            if (e.Emoji.GetDiscordName() == ":heart:")
            {
                UserStatsService statsService = Services.GetRequiredService<UserStatsService>();
                await statsService.ChangeLikeAsync(e.Guild.Id, msg.Author.Id, msg.Id, 1);
                Log.Information("User {Username} ADD heart emoji. MessageId: {Id} AuthorId: {authorId}", e.User.Username, msg.Id, msg.Author.Id);
            }
        }

        public static async Task MessageReactionRemovedEventHandler(DiscordClient client, MessageReactionRemovedEventArgs e)
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
                UserStatsService statsService = Services.GetRequiredService<UserStatsService>();
                await statsService.ChangeLikeAsync(e.Guild.Id, msg.Author.Id, msg.Id, -1);
                Log.Information("User {Username} REMOVE heart emoji. MessageId:{Id} AuthorId:{authorId}", e.User.Username, msg.Id, msg.Author.Id);

            }
        }
    }
}
