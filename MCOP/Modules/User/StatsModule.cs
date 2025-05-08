using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Entities;
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

            var statsService = ctx.ServiceProvider.GetRequiredService<IGuildUserStatsService>();
            GuildUserStats stats = await statsService.GetGuildUserStatAsync(ctx.Guild.Id, member.Id);

            IGuildUserEmojiService guildEmojiService = ctx.ServiceProvider.GetRequiredService<IGuildUserEmojiService>();
            List<GuildUserEmoji> recievedEmojies = await guildEmojiService.GetTopEmojisForUserAsync(ctx.Guild.Id, member.Id);
            StringBuilder topEmoji = new();

            foreach (var item in recievedEmojies)
            {
                var emojiString = (await ctx.Guild.GetEmojiAsync(item.EmojiId)).ToString();
                topEmoji.Append($"{emojiString} {item.RecievedAmount}  ");
            }

            int userLevel = LevelingHelper.GetLevelFromTotalExp(stats.Exp);
            int userRank = await statsService.GetUserExpRankAsync(ctx.Guild.Id, member.Id);

            var embed = new DiscordEmbedBuilder()
            .WithTitle(member.Username)
            .WithThumbnail(member.AvatarUrl)
            .WithDescription(LevelingHelper.GenerateLevelString(userLevel, stats.Exp, userRank))
            .AddField("Лайки", $":heart: {stats.Likes}", true)
            .AddField("Дуели", $":crossed_swords: {stats.DuelWin} - {stats.DuelLose}", true)
            .AddField("Медали за отвагу", recievedEmojies.Count > 0 ? topEmoji.ToString() : DiscordEmoji.FromName(ctx.Client, ":jokerge:").ToString());

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed.Build()));
        }


        [Command("top")]
        [Description("Топ сервера")]
        public async Task StatsTop(CommandContext ctx)
        {
            await ctx.DeferResponseAsync();

            if (ctx.Guild is null)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Guild not found!"));
                return;
            }
            string url = "https://mistercop.top/leaderboard/" + ctx.Guild.Id;
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(url));
        }

        [Command("set-emoji-count")]
        [RequirePermissions(DiscordPermission.Administrator)]
        [Description("Устанавливает кол-во емодзи для юзера (пользователь сначала должен получить этот емодзи хоть раз)")]
        public async Task SetEmojiCount(CommandContext ctx, DiscordUser user, string emojiName, int count)
        {
            await ctx.DeferEphemeralAsync();

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
                var guildEmojiService = ctx.ServiceProvider.GetRequiredService<IGuildUserEmojiService>();
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
            await ctx.DeferEphemeralAsync();

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
                var levelingService = ctx.ServiceProvider.GetRequiredService<IGuildUserStatsService>();
                await levelingService.SetUsersExperienceAsync(ctx.Guild.Id, userIdExp);
            }

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Success!"));
        }
    }
}
