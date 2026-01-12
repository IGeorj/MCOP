using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Entities;
using MCOP.Common.Helpers;
using MCOP.Core.Helpers;
using MCOP.Core.Models;
using MCOP.Core.Services.Image;
using MCOP.Core.Services.Scoped;
using MCOP.Extensions;
using Newtonsoft.Json.Linq;
using System.ComponentModel;
using System.Text;

namespace MCOP.Modules.User
{
    public sealed class StatsModule
    {
        private const string LeaderboardUrl = "https://mistercop.top/leaderboard/";
        private const string Mee6ApiUrl = "https://mee6.xyz/api/plugins/levels/leaderboard/";

        [Command("stats")]
        [Description("Показывает статистику")]
        public async Task Stats(CommandContext ctx,
        [Description("Пользователь")] DiscordUser? user = null)
        {
            await ctx.DeferResponseAsync();

            var (guild, member) = await CommandContextHelper.ValidateAndGetMemberAsync(ctx, user);
            if (guild is null || member is null) return;

            var statsService = ctx.ServiceProvider.GetRequiredService<IGuildUserStatsService>();
            var stats = await statsService.GetGuildUserStatAsync(guild.Id, member.Id);

            var emojiLeaderboard = await BuildEmojiLeaderboardAsync(ctx, guild, member.Id);
            var userRank = await statsService.GetUserExpRankAsync(guild.Id, member.Id);

            var embed = BuildStatsEmbed(member, stats, userRank, emojiLeaderboard);
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
        }


        [Command("top")]
        [Description("Топ сервера")]
        public async Task StatsTopAsync(CommandContext ctx)
        {
            await ctx.DeferResponseAsync();

            var guild = await CommandContextHelper.ValidateAndGetGuildAsync(ctx);
            if (guild is null) return;

            var guildUserStatsService = ctx.ServiceProvider.GetRequiredService<IGuildUserStatsService>();

            var (stats, totalCount) = await guildUserStatsService.GetGuildUserStatsAsync(guild.Id, pageSize: 10);
            var userTopRendered = new UserTopRendered();
            var sKImage = userTopRendered.RenderTable(stats, 1000);
            var sKData = sKImage.Encode(SkiaSharp.SKEncodedImageFormat.Jpeg, 95);

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"[-> Полный список <-](<{LeaderboardUrl}{guild.Id}>)").AddFile("top10.jpg", sKData.AsStream()));
        }

        [Command("sync-mee6-stats")]
        [RequirePermissions(DiscordPermission.Administrator)]
        [Description("Устаналивает лвла, такие же как в mee6")]
        public async Task SyncMee6StatsAsync(CommandContext ctx)
        {
            await ctx.DeferEphemeralAsync();

            var guild = await CommandContextHelper.ValidateAndGetGuildAsync(ctx);
            if (guild is null) return;

            var userIdExp = await FetchMee6LeaderboardDataAsync(ctx);
            if (userIdExp.Count == 0)
            {
                await ctx.EditResponseAsync("No data found!");
                return;
            }

            var levelingService = ctx.ServiceProvider.GetRequiredService<IGuildUserStatsService>();
            await levelingService.SetUsersExperienceAsync(guild.Id, userIdExp);
            await ctx.EditResponseAsync("Success!");
        }


        private async Task<string> BuildEmojiLeaderboardAsync(CommandContext ctx, DiscordGuild guild, ulong userId)
        {
            var reactionService = ctx.ServiceProvider.GetRequiredService<IReactionService>();
            var configService = ctx.ServiceProvider.GetRequiredService<IGuildConfigService>();
            var receivedReactions = await reactionService.GetUserTopReactionsAsync(guild.Id, userId);
            var config = await configService.GetOrAddGuildConfigAsync(guild.Id);

            if (receivedReactions.Count == 0)
                return DiscordEmoji.FromName(ctx.Client, ":jokerge:").ToString();

            var leaderboard = new StringBuilder();
            foreach (var reaction in receivedReactions)
            {
                if (config.LikeEmojiName == reaction.Emoji || config.LikeEmojiId == reaction.EmojiId) continue;

                var emoji = reaction.EmojiId != 0 ? (await guild.GetEmojiAsync(reaction.EmojiId)).ToString() : reaction.Emoji;
                leaderboard.Append($"{emoji} {reaction.Count}  ");
            }

            return leaderboard.ToString();
        }

        private DiscordEmbed BuildStatsEmbed(
            DiscordMember member,
            GuildUserStatsDto stats,
            int rank,
            string emojiLeaderboard)
        {
            return new DiscordEmbedBuilder()
                .WithTitle(member.Username)
                .WithThumbnail(member.AvatarUrl)
                .WithDescription(LevelingHelper.GenerateLevelString(stats.Level, stats.Exp, rank))
                .AddField("Лайки", $":heart: {stats.Likes}", true)
                .AddField("Дуели", $":crossed_swords: {stats.DuelWin} - {stats.DuelLose}", true)
                .AddField("Медали за отвагу", emojiLeaderboard)
                .Build();
        }

        private async Task<Dictionary<ulong, int>> FetchMee6LeaderboardDataAsync(CommandContext ctx)
        {
            var httpClientFactory = ctx.ServiceProvider.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient("mee6Parser");
            var userIdExp = new Dictionary<ulong, int>();

            for (int page = 0; ; page++)
            {
                await Task.Delay(TimeSpan.FromSeconds(1));

                var response = await httpClient.GetAsync($"{Mee6ApiUrl}{ctx.Guild!.Id}?limit=1000&page={page}");
                if (!response.IsSuccessStatusCode) break;

                var json = JObject.Parse(await response.Content.ReadAsStringAsync());
                var playersToken = json.SelectToken("players");
                if (playersToken is null || !playersToken.Any()) break;

                var players = playersToken.Children();

                foreach (var player in players)
                {
                    var userId = player["id"]?.Value<string>();
                    var exp = player["xp"]?.Value<int>();
                    if (userId is null || exp is null) continue;

                    userIdExp[ulong.Parse(userId)] = exp.Value;
                }
            }

            return userIdExp;
        }
    }
}
