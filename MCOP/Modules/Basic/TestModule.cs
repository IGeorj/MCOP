using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Entities;
using MCOP.Data;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace MCOP.Modules.Basic
{
    [Command("test")]
    [RequireApplicationOwner]
    public sealed class TestModule
    {
        [Command("messages")]
        public async Task Messages(CommandContext ctx)
        {
            await ctx.DeferResponseAsync();

            if (ctx.Guild is null)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Guild not found"));
                return;
            }

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Started!"));

            IDbContextFactory<McopDbContext> _contextFactory = ctx.ServiceProvider.GetRequiredService<IDbContextFactory<McopDbContext>>();
            await using var context = await _contextFactory.CreateDbContextAsync();

            var messages = context.GuildMessages.Where(x => x.GuildId == ctx.Guild.Id && (x.CreatedAt == new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) || x.UserId == 0)).ToList();
            var index = 0;
            foreach (var msg in messages)
            {
                try
                {
                    await Task.Delay(400);
                    var channel = await ctx.Guild.GetChannelAsync(msg.ChannelId);
                    var message = await channel.GetMessageAsync(msg.Id);

                    if (message.Author is null)
                        message = await channel.GetMessageAsync(msg.Id, true);

                    if (message.Author is null)
                        continue;

                    msg.UserId = message.Author.Id;
                    msg.CreatedAt = message.CreationTimestamp.UtcDateTime;

                }
                catch (Exception)
                {
                    Log.Information("Failed update message {channelId}, {msgId}", msg.ChannelId, msg.Id);
                    continue;
                }
                Log.Information("Processed {channelId} of {msgId}", index, messages.Count);
                index++;
            }

            context.GuildMessages.UpdateRange(messages);
            context.SaveChanges();
        }
    }
}
