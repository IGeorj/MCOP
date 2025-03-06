using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using MCOP.Common;
using MCOP.Core.Helpers;
using MCOP.Core.Services.Scoped;
using MCOP.Core.ViewModels;
using MCOP.Data.Models;
using MCOP.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
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

            if (ctx.Guild is null)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Guild not found!"));
                return;
            }

            var member = user is null ? ctx.Member : await ctx.Guild.GetMemberAsync(user.Id);

            if (member is null)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Member not found!"));
                return;
            }

            GuildUserStatsService statsService = ctx.ServiceProvider.GetRequiredService<GuildUserStatsService>();
            GuildUserStats stats = await statsService.GetGuildUserStatAsync(ctx.Guild.Id, member.Id);

            GuildUserEmojiService guildEmojiService = ctx.ServiceProvider.GetRequiredService<GuildUserEmojiService>();
            List<GuildUserEmoji> recievedEmojies = await guildEmojiService.GetTopEmojisForUserAsync(ctx.Guild.Id, member.Id);
            StringBuilder topEmoji = new();

            foreach (var item in recievedEmojies)
            {
                var emojiString = (await ctx.Guild.GetEmojiAsync(item.EmojiId)).ToString();
                topEmoji.Append($"{emojiString} {item.RecievedAmount}  ");
            }

            int userLevel = LevelingHelper.GetLevelFromTotalExp(stats.Exp);

            var embed = new DiscordEmbedBuilder()
            .WithTitle(member.Username)
            .WithThumbnail(member.AvatarUrl)
            .WithDescription(LevelingHelper.GenerateLevelString(userLevel, stats.Exp))
            .AddField("Лайки", $":heart: {stats.Likes}", true)
            .AddField("Дуели", $":crossed_swords: {stats.DuelWin} - {stats.DuelLose}", true)
            .AddField("Медали за отвагу", recievedEmojies.Count > 0 ? topEmoji.ToString() : DiscordEmoji.FromName(ctx.Client, ":jokerge:").ToString());

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed.Build()));
        }


        [Command("top")]
        [Description("Топ 5 сервера")]
        public async Task StatsTop(CommandContext ctx)
        {
            await ctx.DeferResponseAsync();

            if (ctx.Guild is null)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Guild not found!"));
                return;
            }

            GuildUserStatsService statsService = ctx.ServiceProvider.GetRequiredService<GuildUserStatsService>();
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
                if (member is not null)
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
                if (member is not null) 
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
                if (member is not null)
                {
                    topCounter++;
                    kek = $"{member.DisplayName}\n:crossed_swords: {item.DuelWin} - {item.DuelLose}";
                    break;
                }
            }

            var embed = new DiscordEmbedBuilder()
            .WithTitle("Топ пользователей")
            .AddField("Топ лайков", topLikes.Length == 0 ? "Пусто" : topLikes.ToString(), true)
            .AddField("Топ дуелей", topDuels.Length == 0 ? "Пусто" : topDuels.ToString(), true)
            .AddField("Невезучий", kek, true);

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed.Build()));
        }

        [Command("set-emoji-count")]
        [RequirePermissions(DiscordPermission.Administrator)]
        [Description("Устанавливает кол-во емодзи для юзера (пользователь сначала должен получить этот емодзи хоть раз)")]
        public async Task SetEmojiCount(CommandContext ctx, DiscordUser user, string emojiName, int count)
        {
            if (ctx is SlashCommandContext slashContext)
            {
                await slashContext.DeferResponseAsync(ephemeral: true);
            }
            else
            {
                await ctx.DeferResponseAsync();
            }

            if (ctx.Guild is null)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Only works on server!"));
                return;
            }

            var emojiParsed = DiscordEmoji.TryFromName(ctx.Client, ":" + emojiName + ":", out DiscordEmoji discordEmoji);

            if (!emojiParsed)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Wrong emoji Name (should be without : :)"));
                return;
            }

            try
            {
                var guildEmojiService = ctx.ServiceProvider.GetRequiredService<GuildUserEmojiService>();
                await guildEmojiService.SetGuildUserEmojiCountAsync(ctx.Guild.Id, user.Id, discordEmoji, count);

            }
            catch (Exception)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Error!"));
                return;
            }

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Success!"));
        }


        [Command("sync-mee6-stats")]
        [RequirePermissions(DiscordPermission.Administrator)]
        [Description("Устаналивает лвла, такие же как в mee6")]
        public async Task SyncMee6Stats(CommandContext ctx)
        {
            if (ctx is SlashCommandContext slashContext)
            {
                await slashContext.DeferResponseAsync(ephemeral: true);
            }
            else
            {
                await ctx.DeferResponseAsync();
            }

            var httpClientFactory = ctx.ServiceProvider.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient("mee6Parser");

            var isAllParsed = false;
            var page = 0;
            var userIdExp = new Dictionary<ulong, int>();

            do
            {
                Thread.Sleep(TimeSpan.FromSeconds(1));

                var response = await httpClient.GetAsync($"https://mee6.xyz/api/plugins/levels/leaderboard/{ctx.Guild?.Id}?limit=1000&page={page}");

                if (!response.IsSuccessStatusCode)
                {
                    isAllParsed = true;
                    break;
                }

                JObject json = JObject.Parse(await response.Content.ReadAsStringAsync());
                var playersToken = json.SelectToken("players");

                if (playersToken is null || !playersToken.Any())
                {
                    isAllParsed = true;
                    break;
                }
                var players = playersToken.Children();
                foreach (var item in players)
                {
                    var userId = item["id"]?.Value<string>();
                    var exp = item["xp"]?.Value<int>();

                    if (userId is null || exp is null) break;

                    userIdExp.Add(ulong.Parse(userId), exp.Value);
                }

                page += 1;
            } while (!isAllParsed);

            if (isAllParsed && userIdExp.Any() && ctx.Guild is not null)
            {
                var levelingService = ctx.ServiceProvider.GetRequiredService<GuildUserStatsService>();
                await levelingService.SetUsersExperienceAsync(ctx.Guild.Id, userIdExp);
            }

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Success!"));
        }
    }
}
