using DSharpPlus;
using MCOP.Core.Common;
using MCOP.Core.Helpers;
using MCOP.Core.Models;
using MCOP.Core.Services.Singletone;
using MCOP.Data;
using MCOP.Data.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace MCOP.Core.Services.Scoped
{
    public interface IGuildUserStatsService
    {
        public Task<GuildUserStats> GetGuildUserStatAsync(ulong guildId, ulong userId);
        public Task<(List<GuildUserStatsDto> stats, int totalCount)> GetGuildUserStatsAsync(ulong guildId, int page = 1, int pageSize = 20, string? sortBy = null, bool sortDescending = true);
        public Task<int> GetUserExpRankAsync(ulong guildId, ulong userId);
        public Task AddLikeAsync(ulong guildId, ulong userId);
        public Task RemoveLikeAsync(ulong guildId, ulong userId);
        public Task UpdateUserInfo(ulong guildId, ulong userId, string userName, string avatarHash);
        public Task AddMessageExpAsync(ulong guildId, ulong channelId, ulong userId);
        public Task AddExpAsync(ulong guildId, ulong channelId, ulong userId, int exp);
        public Task RemoveExpAsync(ulong guildId, ulong channelId, ulong userId, int exp);
        public Task AddDuelWinAsync(ulong guildId, ulong userId);
        public Task AddDuelLoseAsync(ulong guildId, ulong userId);
        public Task SetUsersExperienceAsync(ulong guildId, Dictionary<ulong, int> userIdExp);
        public Task UpdateMissingUserInfoAsync(ulong guildId, List<GuildUserStatsDto> guildUserStats);
    }

    public class GuildUserStatsService : IGuildUserStatsService
    {
        public readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(5);

        private readonly IDbContextFactory<McopDbContext> _contextFactory;
        private readonly ILockingService _lockingService;
        private readonly IGuildRoleService _guildRoleService;
        private readonly DiscordClient _discordClient;

        private const int ExpCooldownMinutes = 1;
        private const int MinRandomExp = 15;
        private const int MaxRandomExp = 26;

        public GuildUserStatsService(IDbContextFactory<McopDbContext> contextFactory, ILockingService lockingService, IGuildRoleService guildRoleService, DiscordClient discordClient)
        {
            _contextFactory = contextFactory;
            _lockingService = lockingService;
            _guildRoleService = guildRoleService;
            _discordClient = discordClient;
        }

        #region public

        public async Task<GuildUserStats> GetGuildUserStatAsync(ulong guildId, ulong userId)
        {
            await using var context = _contextFactory.CreateDbContext();

            try
            {
                var userStats = await context.GuildUserStats
                    .AsNoTracking()
                    .FirstOrDefaultAsync(us => us.GuildId == guildId && us.UserId == userId);

                if (userStats == null)
                {
                    userStats = new GuildUserStats
                    {
                        GuildId = guildId,
                        UserId = userId,
                        Likes = 0,
                        Exp = 0,
                        DuelWin = 0,
                        DuelLose = 0,
                        LastExpAwardedAt = DateTime.MinValue
                    };
                    context.GuildUserStats.Add(userStats);
                    await context.SaveChangesAsync();
                }

                Log.Information("GetGuildUserStatAsync: Retrieved stats for guildId: {guildId}, userId: {userId}", guildId, userId);
                return userStats;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in GetGuildUserStatAsync for guildId: {guildId}, userId: {userId}", guildId, userId);
                throw;
            }
        }

        public async Task<(List<GuildUserStatsDto> stats, int totalCount)> GetGuildUserStatsAsync(ulong guildId, int page = 1, int pageSize = 20, string? sortBy = null, bool sortDescending = true)
        {
            try
            {
                var guildIdStr = guildId.ToString();

                await using var context = _contextFactory.CreateDbContext();

                var query = context.GuildUserStats
                    .Where(x => x.GuildId == guildId)
                    .AsQueryable();

                var totalCount = await query.CountAsync();

                query = (sortBy?.ToLower()) switch
                {
                    "likes" => sortDescending
                        ? query.OrderByDescending(x => x.Likes)
                        : query.OrderBy(x => x.Likes),
                    "duelwin" => sortDescending
                        ? query.OrderByDescending(x => x.DuelWin)
                        : query.OrderBy(x => x.DuelWin),
                    "duellose" => sortDescending
                        ? query.OrderByDescending(x => x.DuelLose)
                        : query.OrderBy(x => x.DuelLose),
                    _ => sortDescending
                        ? query.OrderByDescending(x => x.Exp)
                        : query.OrderBy(x => x.Exp),
                };

                var pagedQuery = query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize);

                var stats = await pagedQuery.AsNoTracking().ToListAsync();

                var rankedStats = stats
                    .Select((x, index) => new GuildUserStatsDto
                    {
                        GuildId = x.GuildId.ToString(),
                        UserId = x.UserId.ToString(),
                        Username = x.Username,
                        AvatarHash = x.AvatarHash,
                        DuelWin = x.DuelWin,
                        DuelLose = x.DuelLose,
                        Likes = x.Likes,
                        Exp = x.Exp,
                    })
                    .ToList();

                Log.Information("GetGuildUserStatsAsync for guildId: {guildId}, page: {page}, pageSize: {pageSize}, sortBy: {sortBy}, sortDescending: {sortDescending}", guildId, page, pageSize, sortBy ?? "exp", sortDescending);

                return (rankedStats, totalCount);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in GetGuildUserStatsAsync for guildId: {guildId}, page: {page}, pageSize: {pageSize}, sortBy: {sortBy}, sortDescending: {sortDescending}", guildId, page, pageSize, sortBy ?? "exp", sortDescending);
                throw;
            }
        }

        public async Task<int> GetUserExpRankAsync(ulong guildId, ulong userId)
        {
            try
            {
                await using var context = _contextFactory.CreateDbContext();

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

        public async Task AddLikeAsync(ulong guildId, ulong userId)
        {
            await ModifyUserStatsAsync(guildId, userId, userStats => userStats.Likes++);
        }

        public async Task RemoveLikeAsync(ulong guildId, ulong userId)
        {
            await ModifyUserStatsAsync(guildId, userId, userStats =>
            {
                if (userStats.Likes > 0)
                    userStats.Likes--;
            });
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
            await using var context = _contextFactory.CreateDbContext();

            var userStats = await context.GuildUserStats
                .SingleOrDefaultAsync(us => us.GuildId == guildId && us.UserId == userId);

            if (userStats != null && userStats.IsWithinExpCooldown(userStats.LastExpAwardedAt, ExpCooldownMinutes))
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
                    var member = await  guild.GetMemberAsync(ulong.Parse(user.UserId), false);
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
                            user.Username = discordUser.GlobalName ?? user.Username;
                            user.AvatarHash = discordUser.AvatarHash ?? user.AvatarHash;
                        }

                    }
                    catch (Exception)
                    {
                        user.Username = $"Unknown";
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
            var userStats = await context.GuildUserStats
                .SingleOrDefaultAsync(us => us.GuildId == guildId && us.UserId == userId);

            if (userStats == null)
            {
                userStats = new GuildUserStats
                {
                    GuildId = guildId,
                    UserId = userId,
                    Likes = 0,
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
                await using var context = _contextFactory.CreateDbContext();

                try
                {
                    var userStats = await GetOrCreateUserStatsInternalAsync(context, guildId, userId);

                    var originalValues = LogHelper.GetClassProperties(userStats);

                    modifyAction(userStats);

                    var updatedValues = LogHelper.GetClassProperties(userStats);

                    LogHelper.LogChangedProperties(originalValues, updatedValues, guildId, userId);

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
                await _guildRoleService.UpdateLevelRolesAsync(guildId, channelId, userId, oldLevel, newLevel);
        }

        private async Task BatchUpdateUserInfoAsync(List<GuildUserStatsDto> usersToUpdate)
        {
            await using var context = _contextFactory.CreateDbContext();

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
