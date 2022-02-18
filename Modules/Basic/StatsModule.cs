using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using MCOP.Attributes.SlashCommands;
using MCOP.Database.Models;
using MCOP.Extensions;
using MCOP.Modules.Basic.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Text;

namespace MCOP.Modules.Basic
{
    [SlashCooldown(2, 5, CooldownBucketType.Channel)]
    public sealed class StatsModule : ApplicationCommandModule
    {
        [SlashCommand("stats", "Показывает статистику")]
        public async Task Stats(InteractionContext ctx,
            [Option("user", "Пользователь")] DiscordUser? user = null)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            var member = user is null ? ctx.Member : user as DiscordMember;

            UserStatsService statsService = ctx.Services.GetRequiredService<UserStatsService>();
            UserStats stats = await statsService.GetOrAddAsync(ctx.Guild.Id, member.Id);

            var embed = new DiscordEmbedBuilder()
            {
                Title = member.ToDiscriminatorString(),
            }
            .WithThumbnail(member.AvatarUrl)
            .AddField("Лайки", $":heart: {stats.Like}", true)
            .AddField("Дуели", $":crossed_swords: {stats.DuelWin} - {stats.DuelLose}", true);

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed.Build()));
        }
        
        [SlashCommand("top", "Топ 5 сервера")]
        public async Task StatsTop(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            UserStatsService statsService = ctx.Services.GetRequiredService<UserStatsService>();
            var likeStats = await statsService.GetTopLikedUsersAsync(ctx.Guild.Id);
            var duelStats = await statsService.GetTopDuelUsersAsync(ctx.Guild.Id);
            var kekStats = await statsService.GetKekDuelUserAsync(ctx.Guild.Id);

            StringBuilder topLikes = new();
            foreach (var item in likeStats)
            {
                topLikes.Append($"<@!{item.UserId}>\n:heart: {item.Like}\n");
            }

            StringBuilder topDuels = new();
            foreach (var item in duelStats)
            {
                topDuels.Append($"<@!{item.UserId}>\n:crossed_swords: {item.DuelWin} - {item.DuelLose}\n");
            }

            string kek = "???";
            if (kekStats is not null)
            {
                kek = $"<@!{kekStats.UserId}>\n:crossed_swords: {kekStats.DuelWin} - {kekStats.DuelLose}";
            }


            var embed = new DiscordEmbedBuilder()
            {
                Title = "Топ пользователей"
            }
            .AddField("Топ лайков", topLikes.ToString(), true)
            .AddField("Топ дуелей", topDuels.ToString(), true)
            .AddField("Невезучий", kek, true);

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed.Build()));
        }
    }
}
