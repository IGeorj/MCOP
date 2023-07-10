using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using MCOP.Core.Services.Scoped;
using MCOP.Core.ViewModels;
using MCOP.Data.Models;
using MCOP.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System.Text;

namespace MCOP.Modules.User
{
    [SlashCooldown(2, 5, SlashCooldownBucketType.Channel)]
    public sealed class StatsModule : ApplicationCommandModule
    {
        [SlashCommand("stats", "Показывает статистику")]
        public async Task Stats(InteractionContext ctx,
        [Option("user", "Пользователь")] DiscordUser? user = null)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            var member = user is null ? ctx.Member : user as DiscordMember;
            if (member is null)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Пользователь не найден"));
                return;
            }

            UserStatsService statsService = ctx.Services.GetRequiredService<UserStatsService>();
            GuildUserStat stats = await statsService.GetOrAddAsync(ctx.Guild.Id, member.Id);

            var embed = new DiscordEmbedBuilder()
            .WithTitle(member.ToDiscriminatorString())
            .WithThumbnail(member.AvatarUrl)
            .AddField("Лайки", $":heart: {stats.Likes}", true)
            .AddField("Дуели", $":crossed_swords: {stats.DuelWin} - {stats.DuelLose}", true);

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed.Build()));
        }


        [SlashCommand("top", "Топ 5 сервера")]
        public async Task StatsTop(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            UserStatsService statsService = ctx.Services.GetRequiredService<UserStatsService>();
            ServerTopVM serverTop = await statsService.GetServerTopAsync(ctx.Guild.Id);

            StringBuilder topLikes = new();
            StringBuilder topDuels = new();
            string kek = "5 дуелей минимум";

            foreach (var item in serverTop.TopLikedUser)
            {
                DiscordMember? member = await ctx.Guild.GetMemberSilentAsync(item.UserId);
                if (member != null)
                {
                    topLikes.Append($"{member.DisplayName}\n:heart: {item.Likes}\n");
                }
            }

            foreach (var item in serverTop.TopDuelUser)
            {
                DiscordMember? member = await ctx.Guild.GetMemberSilentAsync(item.UserId);
                if (member != null)
                {
                    topDuels.Append($"{member.DisplayName}\n:crossed_swords: {item.DuelWin} - {item.DuelLose}\n");
                }
            }

            if (serverTop.HonorableMention is not null)
            {
                DiscordMember? member = await ctx.Guild.GetMemberSilentAsync(serverTop.HonorableMention.UserId);
                if (member != null)
                {
                    kek = $"{member.DisplayName}\n:crossed_swords: {serverTop.HonorableMention.DuelWin} - {serverTop.HonorableMention.DuelLose}";
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
