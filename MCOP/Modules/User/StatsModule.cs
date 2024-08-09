using DSharpPlus.Commands;
using DSharpPlus.Entities;
using MCOP.Core.Services.Scoped;
using MCOP.Core.ViewModels;
using MCOP.Data.Models;
using MCOP.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.Text;

namespace MCOP.Modules.User
{
    public sealed class StatsModule
    {
        [Command("stats")]
        [Description("Показывает статистику")]
        public async Task Stats(CommandContext ctx,
        [Description("Пользователь")] DiscordUser? user = null)
        {
            await ctx.DeferResponseAsync();

            var member = user is null ? ctx.Member : user as DiscordMember;
            if (member is null)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Пользователь не найден"));
                return;
            }

            UserStatsService statsService = ctx.ServiceProvider.GetRequiredService<UserStatsService>();
            GuildUserStat stats = await statsService.GetOrAddAsync(ctx.Guild.Id, member.Id);

            var embed = new DiscordEmbedBuilder()
            .WithTitle(member.Username)
            .WithThumbnail(member.AvatarUrl)
            .AddField("Лайки", $":heart: {stats.Likes}", true)
            .AddField("Дуели", $":crossed_swords: {stats.DuelWin} - {stats.DuelLose}", true);

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed.Build()));
        }


        [Command("top")]
        [Description("Топ 5 сервера")]
        public async Task StatsTop(CommandContext ctx)
        {
            await ctx.DeferResponseAsync();

            UserStatsService statsService = ctx.ServiceProvider.GetRequiredService<UserStatsService>();
            ServerTopVM serverTop = await statsService.GetServerTopAsync(ctx.Guild.Id);

            StringBuilder topLikes = new();
            StringBuilder topDuels = new();
            string kek = "5 дуелей минимум";

            int topCounter = 0;
            int maxTopCount = 5;
            foreach (var item in serverTop.TopLikedUser)
            {
                if (topCounter >= maxTopCount)
                {
                    break;
                }

                DiscordMember? member = await ctx.Guild.GetMemberSilentAsync(item.UserId);
                if (member != null)
                {
                    topCounter++;
                    topLikes.Append($"{member.DisplayName}\n:heart: {item.Likes}\n");
                }
            }

            topCounter = 0;
            foreach (var item in serverTop.TopDuelUser)
            {
                if (topCounter >= maxTopCount)
                {
                    break;
                }

                DiscordMember? member = await ctx.Guild.GetMemberSilentAsync(item.UserId);
                if (member != null)
                {
                    topCounter++;
                    topDuels.Append($"{member.DisplayName}\n:crossed_swords: {item.DuelWin} - {item.DuelLose}\n");
                }
            }

            topCounter = 0;
            foreach (var item in serverTop.HonorableMention)
            {
                if (topCounter >= 1)
                {
                    break;
                }

                DiscordMember? member = await ctx.Guild.GetMemberSilentAsync(item.UserId);
                if (member != null)
                {
                    topCounter++;
                    kek = $"{member.DisplayName}\n:crossed_swords: {item.DuelWin} - {item.DuelLose}";
                    break;
                }
            }

            var embed = new DiscordEmbedBuilder()
            .WithTitle("Топ пользователей")
            .AddField("Топ лайков", topLikes.ToString(), true)
            .AddField("Топ дуелей", topDuels.ToString(), true)
            .AddField("Невезучий", kek, true);

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed.Build()));
        }
    }
}
