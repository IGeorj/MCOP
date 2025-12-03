using DSharpPlus;
using MCOP.Core.Common;
using MCOP.Core.Helpers;
using MCOP.Core.Models;
using MCOP.Core.Services.Singletone;
using MCOP.Data;
using MCOP.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Serilog;
using System.Data;

namespace MCOP.Core.Services.Scoped
{
    public interface IGuildUserStatsService
    {
        public Task<GuildUserStatsDto> GetGuildUserStatAsync(ulong guildId, ulong userId);
        public Task<(List<GuildUserStatsDto> stats, int totalCount)> GetGuildUserStatsAsync(ulong guildId, int page = 1, int pageSize = 20, string? sortBy = null, bool sortDescending = true);
        public Task<int> GetUserExpRankAsync(ulong guildId, ulong userId);
        public Task UpdateUserInfo(ulong guildId, ulong userId, string userName, string avatarHash);
        public Task AddMessageExpAsync(ulong guildId, ulong channelId, ulong userId);
        public Task AddExpAsync(ulong guildId, ulong channelId, ulong userId, int exp);
        public Task RemoveExpAsync(ulong guildId, ulong channelId, ulong userId, int exp);
        public Task AddDuelWinAsync(ulong guildId, ulong userId);
        public Task AddDuelLoseAsync(ulong guildId, ulong userId);
        public Task SetUsersExperienceAsync(ulong guildId, Dictionary<ulong, int> userIdExp);
        public Task UpdateMissingUserInfoAsync(ulong guildId, List<GuildUserStatsDto> guildUserStats);
    }

    public sealed class GuildUserStatsService : IGuildUserStatsService
    {
        public static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(5);
        private const int ExpCooldownMinutes = 1;
        private const int MinRandomExp = 15;
        private const int MaxRandomExp = 26;

        private readonly IDbContextFactory<McopDbContext> _contextFactory;
        private readonly ILockingService _lockingService;
        private readonly IGuildRoleService _guildRoleService;
        private readonly IGuildConfigService _guildConfigService;
        private readonly IReactionService _reactionService;
        private readonly IMemoryCache _cache;
        private readonly DiscordClient _discordClient;


        public GuildUserStatsService(
            IDbContextFactory<McopDbContext> contextFactory, ILockingService lockingService,
            IGuildRoleService guildRoleService, DiscordClient discordClient, IMemoryCache memoryCache,
            IReactionService reactionService, IGuildConfigService guildConfigService)
        {
            _contextFactory = contextFactory;
            _lockingService = lockingService;
            _guildRoleService = guildRoleService;
            _discordClient = discordClient;
            _cache = memoryCache;
            _reactionService = reactionService;
            _guildConfigService = guildConfigService;
        }

        #region public

        public async Task<GuildUserStatsDto> GetGuildUserStatAsync(ulong guildId, ulong userId)
        {
            await using var context = _contextFactory.CreateDbContext();

            try
            {
                var userStats = await context.GuildUserStats
                    .FirstOrDefaultAsync(us => us.GuildId == guildId && us.UserId == userId);

                if (userStats == null)
                {
                    userStats = new GuildUserStats
                    {
                        GuildId = guildId,
                        UserId = userId,
                        Exp = 0,
                        DuelWin = 0,
                        DuelLose = 0,
                        LastExpAwardedAt = DateTime.MinValue
                    };
                    context.GuildUserStats.Add(userStats);
                    await context.SaveChangesAsync();
                }

                var guildConfig = await _guildConfigService.GetOrAddGuildConfigAsync(guildId);
                var customLikeCount = await _reactionService.GetUserReactionsCount(guildId, userId, guildConfig.LikeEmojiName, guildConfig.LikeEmojiId);

                if (guildConfig.LikeEmojiName == "❤️")
                    customLikeCount += userStats.Likes;

                Log.Information("GetGuildUserStatAsync: Retrieved (or created) stats for guildId: {guildId}, userId: {userId}", guildId, userId);

                return new GuildUserStatsDto
                {
                    GuildId = userStats.GuildId.ToString(),
                    UserId = userStats.UserId.ToString(),
                    Username = userStats.Username,
                    AvatarHash = userStats.AvatarHash,
                    DuelWin = userStats.DuelWin,
                    DuelLose = userStats.DuelLose,
                    Exp = userStats.Exp,
                    Likes = customLikeCount,
                    CustomLikeEmojiId = guildConfig.LikeEmojiId,
                    CustomLikeEmojiName = guildConfig.LikeEmojiName
                };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in GetGuildUserStatAsync for guildId: {guildId}, userId: {userId}", guildId, userId);
                throw;
            }
        }

        public async Task<(List<GuildUserStatsDto> stats, int totalCount)> GetGuildUserStatsAsync(
            ulong guildId,
            int page = 1,
            int pageSize = 20,
            string? sortBy = null,
            bool sortDescending = true)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);
            var offset = (page - 1) * pageSize;
            var sort = sortBy?.ToLowerInvariant() switch
            {
                "likes" => "Likes",
                "duelwin" => "DuelWin",
                "duellose" => "DuelLose",
                _ => "Exp"
            };

            var orderDir = sortDescending ? "DESC" : "ASC";

            await using var context = await _contextFactory.CreateDbContextAsync();
            var guildConfig = await _guildConfigService.GetOrAddGuildConfigAsync(guildId);

            var likeEmoji = guildConfig.LikeEmojiName;
            var likeEmojiId = guildConfig.LikeEmojiId;

            var totalCount = await context.GuildUserStats
                .CountAsync(s => s.GuildId == guildId);

            var orderByClause = $"ORDER BY {sort} {orderDir}";

            var sql = $@"
                SELECT 
                    s.GuildId,
                    s.UserId,
                    s.Username,
                    s.AvatarHash,
                    s.DuelWin,
                    s.DuelLose,
                    s.Exp,
                    (COALESCE(r.ReactionCount, 0) + 
                        CASE WHEN {{0}} = '❤️' THEN s.Likes ELSE 0 END
                    ) AS Likes
                FROM GuildUserStats s
                LEFT JOIN (
                    SELECT 
                        m.UserId,
                        COUNT(*) AS ReactionCount
                    FROM GuildMessageReactions r
                    INNER JOIN GuildMessages m ON r.MessageId = m.Id
                    WHERE 
                        r.GuildId = {{1}} 
                        AND r.Emoji = {{2}} 
                        AND r.EmojiId = {{3}}
                    GROUP BY m.UserId
                ) r ON r.UserId = s.UserId
                WHERE s.GuildId = {{1}}
                {orderByClause}
                LIMIT {{4}} OFFSET {{5}}";

            var projections = await context.Set<GuildUserStatsProjection>()
                .FromSqlRaw(sql, likeEmoji, guildId, likeEmoji, likeEmojiId, pageSize, offset)
                .AsNoTracking()
                .ToListAsync();

            var results = projections.Select(p => new GuildUserStatsDto
            {
                GuildId = p.GuildId.ToString(),
                UserId = p.UserId.ToString(),
                Username = p.Username ?? "Unknown",
                AvatarHash = p.AvatarHash,
                DuelWin = p.DuelWin,
                DuelLose = p.DuelLose,
                Exp = p.Exp,
                Likes = p.Likes
            }).ToList();

            Log.Information("Fetched {Count} stats for guild {GuildId}, page {Page}", results.Count, guildId, page);
            return (results, totalCount);
        }

        public async Task<int> GetUserExpRankAsync(ulong guildId, ulong userId)
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();

                var rank = await context.GuildUserStats
                    .AsNoTracking()
                    .Where(x => x.GuildId == guildId)
                    .CountAsync(x => x.Exp > context.GuildUserStats
                        .Where(y => y.GuildId == guildId && y.UserId == userId)
                        .Select(y => y.Exp)
                        .FirstOrDefault()) + 1;

                Log.Information("GetUserExpRankAsync for guildId: {guildId}, userId:{userId}", guildId, userId);

                return rank;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in GetUserExpRankAsync for guildId: {guildId}, userId:{userId}", guildId, userId);
                throw;
            }
        }

        public async Task UpdateUserInfo(ulong guildId, ulong userId, string userName, string avatarHash)
        {
            await ModifyUserStatsAsync(guildId, userId, userStats =>
            {
                userStats.AvatarHash = avatarHash;
                userStats.Username = userName;
            });
        }

        public async Task AddMessageExpAsync(ulong guildId, ulong channelId, ulong userId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var userStats = await context.GuildUserStats
                .SingleOrDefaultAsync(us => us.GuildId == guildId && us.UserId == userId);

            if (userStats != null && userStats.IsWithinExpCooldown(ExpCooldownMinutes))
                return;

            var blockedRoles = await _guildRoleService.GetBlockedExpGuildRolesAsync(guildId);
            if (blockedRoles.Count > 0)
            {
                var guild = await _discordClient.GetGuildAsync(guildId);
                var user = await guild.GetMemberAsync(userId);

                if (user.Roles.Any(userRole => blockedRoles.Select(x => x.Id).Contains(userRole.Id)))
                    return;
            }

            var gainedExp = new SafeRandom().Next(MinRandomExp, MaxRandomExp);
            await ProcessExpChangeAsync(guildId, channelId, userId, gainedExp, updateLastAwarded: true);
        }

        public async Task AddExpAsync(ulong guildId, ulong channelId, ulong userId, int exp)
        {
            await ProcessExpChangeAsync(guildId, channelId, userId, exp);
        }

        public async Task RemoveExpAsync(ulong guildId, ulong channelId, ulong userId, int exp)
        {
            await ProcessExpChangeAsync(guildId, channelId, userId, -exp);
        }

        public async Task AddDuelWinAsync(ulong guildId, ulong userId)
        {
            await ModifyUserStatsAsync(guildId, userId, userStats => userStats.DuelWin++);
        }

        public async Task AddDuelLoseAsync(ulong guildId, ulong userId)
        {
            await ModifyUserStatsAsync(guildId, userId, userStats => userStats.DuelLose++);
        }

        public async Task SetUsersExperienceAsync(ulong guildId, Dictionary<ulong, int> userIdExp)
        {
            await using var context = _contextFactory.CreateDbContext();

            try
            {
                var userIds = userIdExp.Keys.ToList();
                var existingStats = await context.GuildUserStats
                    .Where(us => us.GuildId == guildId && userIds.Contains(us.UserId))
                    .ToListAsync();

                var statsToUpdate = new List<GuildUserStats>();
                var statsToAdd = new List<GuildUserStats>();

                foreach (var (userId, exp) in userIdExp)
                {
                    var stats = existingStats.FirstOrDefault(us => us.UserId == userId);
                    if (stats is null)
                    {
                        statsToAdd.Add(new GuildUserStats
                        {
                            GuildId = guildId,
                            UserId = userId,
                            Exp = exp,
                            LastExpAwardedAt = DateTime.UtcNow
                        });
                    }
                    else
                    {
                        stats.Exp = exp;
                        statsToUpdate.Add(stats);
                    }
                }

                if (statsToUpdate.Count != 0)
                    context.GuildUserStats.UpdateRange(statsToUpdate);

                if (statsToAdd.Count != 0)
                    await context.GuildUserStats.AddRangeAsync(statsToAdd);

                await context.SaveChangesAsync();

                Log.Information("SetUsersExperienceAsync: Updated {updatedCount} and added {addedCount} records for guildId: {guildId}",
                    statsToUpdate.Count, statsToAdd.Count, guildId);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in SetUsersExperienceAsync for guildId: {guildId}", guildId);
                throw;
            }
        }

        public async Task UpdateMissingUserInfoAsync(ulong guildId, List<GuildUserStatsDto> guildUserStats)
        {
            var allUsers = guildUserStats.Where(u => string.IsNullOrEmpty(u.AvatarHash) || string.IsNullOrEmpty(u.Username)).ToList();

            if (allUsers.Count == 0) return;

            var guild = await _discordClient.GetGuildAsync(guildId);
            foreach (var user in allUsers)
            {
                try
                {
                    var member = await guild.GetMemberAsync(ulong.Parse(user.UserId), false);
                    if (member is not null)
                    {
                        user.Username = member.DisplayName ?? user.Username;
                        user.AvatarHash = member.AvatarHash ?? user.AvatarHash;
                    }
                }
                catch
                {
                    try
                    {
                        var discordUser = await _discordClient.GetUserAsync(ulong.Parse(user.UserId));
                        if (discordUser is not null)
                        {
                            user.Username = discordUser.GlobalName ?? user.Username ?? discordUser.Username ?? "Unknown";
                            user.AvatarHash = discordUser.AvatarHash ?? user.AvatarHash ?? "default";
                        }

                    }
                    catch (Exception)
                    {
                        user.Username = "Unknown";
                        user.AvatarHash = "default";
                    }
                }
            }

            await BatchUpdateUserInfoAsync(allUsers);
        }

        #endregion public


        #region private

        private async Task<GuildUserStats> GetOrCreateUserStatsInternalAsync(McopDbContext context, ulong guildId, ulong userId)
        {
            GuildUserStats? userStats = null;

            if (!_cache.TryGetValue((nameof(GuildUserStats), guildId, userId), out userStats))
            {
                userStats = await context.GuildUserStats
                    .SingleOrDefaultAsync(us => us.GuildId == guildId && us.UserId == userId);
            }
            else if (userStats is not null)
            {
                context.GuildUserStats.Attach(userStats);
            }

            if (userStats is null)
            {
                userStats = new GuildUserStats
                {
                    GuildId = guildId,
                    UserId = userId,
                    Exp = 0,
                    DuelWin = 0,
                    DuelLose = 0,
                    LastExpAwardedAt = DateTime.MinValue,
                    Username = string.Empty,
                    AvatarHash = string.Empty
                };
                context.GuildUserStats.Add(userStats);
            }

            return userStats;
        }

        private async Task ModifyUserStatsAsync(ulong guildId, ulong userId, Action<GuildUserStats> modifyAction)
        {
            var key = ("ModifyUserStatsAsync", guildId, userId);

            using (await _lockingService.AcquireLockAsync(key))
            {
                await using var context = await _contextFactory.CreateDbContextAsync();

                try
                {
                    var userStats = await GetOrCreateUserStatsInternalAsync(context, guildId, userId);

                    var originalValues = LogHelper.GetClassProperties(userStats);

                    modifyAction(userStats);

                    var updatedValues = LogHelper.GetClassProperties(userStats);

                    LogHelper.LogChangedProperties(originalValues, updatedValues, guildId, userId);

                    _cache.Set((nameof(GuildUserStats), guildId, userId), userStats, new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                    });

                    await context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error in ModifyUserStatAsync for guildId: {guildId}, userId: {userId}", guildId, userId);
                    throw;
                }
            }
        }

        private async Task ProcessExpChangeAsync(ulong guildId, ulong channelId, ulong userId, int expChange, bool updateLastAwarded = false)
        {
            int oldLevel = 0;
            int newLevel = 0;

            await ModifyUserStatsAsync(guildId, userId, userStats =>
            {
                oldLevel = LevelingHelper.GetLevelFromTotalExp(userStats.Exp);
                userStats.Exp = Math.Max(0, userStats.Exp + expChange);

                if (updateLastAwarded)
                    userStats.LastExpAwardedAt = DateTime.UtcNow;

                newLevel = LevelingHelper.GetLevelFromTotalExp(userStats.Exp);
            });

            if (oldLevel != newLevel)
                await _guildRoleService.ApplyLevelingRolesAsync(guildId, channelId, userId, oldLevel, newLevel);
        }

        private async Task BatchUpdateUserInfoAsync(List<GuildUserStatsDto> usersToUpdate)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            foreach (var user in usersToUpdate)
            {
                try
                {
                    await context.GuildUserStats
                        .Where(u => u.GuildId == ulong.Parse(user.GuildId) && u.UserId == ulong.Parse(user.UserId))
                        .ExecuteUpdateAsync(setters => setters
                            .SetProperty(u => u.Username, user.Username)
                            .SetProperty(u => u.AvatarHash, user.AvatarHash));
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error in BatchUpdateUserInfoAsync UserId: {UserId}, GuildId:{GuildId}", user.UserId, user.GuildId);
                }
            }
        }

        #endregion private
    }
}
