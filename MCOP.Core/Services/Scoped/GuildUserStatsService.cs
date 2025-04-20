using DSharpPlus;
using MCOP.Core.Common;
using MCOP.Core.Exceptions;
using MCOP.Core.Helpers;
using MCOP.Core.Services.Singletone;
using MCOP.Core.ViewModels;
using MCOP.Data;
using MCOP.Data.Models;
using MCOP.Utils.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Serilog;

namespace MCOP.Core.Services.Scoped
{
    public class GuildUserStatsService : IScoped
    {
        public readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(5);

        private readonly IDbContextFactory<McopDbContext> _contextFactory;
        private readonly ILockingService _lockingService;
        private readonly GuildRoleService _guildRoleService;

        private const int ExpCooldownMinutes = 1;
        private const int MinRandomExp = 15;
        private const int MaxRandomExp = 26;

        public GuildUserStatsService(IDbContextFactory<McopDbContext> contextFactory, ILockingService lockingService, GuildRoleService guildRoleService)
        {
            _contextFactory = contextFactory;
            _lockingService = lockingService;
            _guildRoleService = guildRoleService;
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

                return rank;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in GetUserExpRankAsync for guildId: {guildId}, userId:{userId}", guildId, userId);
                throw;
            }
        }

        public async Task<ServerTopVM> GetServerTopAsync(ulong guildId)
        {
            try
            {
                await using var context = _contextFactory.CreateDbContext();

                var topByLikes = await context.GuildUserStats
                    .Where(x => x.GuildId == guildId)
                    .OrderByDescending(x => x.Likes)
                    .Take(20)
                    .ToListAsync();

                var topDuels = await context.GuildUserStats
                    .Where(x => x.GuildId == guildId)
                    .OrderByDescending(x => x.DuelWin)
                    .Take(20)
                    .ToListAsync();

                var honorableMention = await context.GuildUserStats
                    .Where(x => x.GuildId == guildId)
                    .OrderByDescending(u => u.DuelLose - u.DuelWin)
                    .Take(20)
                    .ToListAsync();

                Log.Information("GetServerTopAsync guildId: {guildId}", guildId);

                return new ServerTopVM
                {
                    TopLikedUser = topByLikes,
                    TopDuelUser = topDuels,
                    HonorableMention = honorableMention
                };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in GetServerTopAsync for guildId: {guildId}", guildId);
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

        public async Task AddMessageExpAsync(DiscordClient client, ulong guildId, ulong channelId, ulong userId)
        {
            await using var context = _contextFactory.CreateDbContext();

            var userStats = await context.GuildUserStats
                .SingleOrDefaultAsync(us => us.GuildId == guildId && us.UserId == userId);

            if (userStats != null && userStats.IsWithinExpCooldown(userStats.LastExpAwardedAt, ExpCooldownMinutes))
                return;

            var blockedRoles = await _guildRoleService.GetBlockedExpGuildRolesAsync(guildId);
            if (blockedRoles.Count > 0)
            {
                var guild = await client.GetGuildAsync(guildId);
                var user = await guild.GetMemberAsync(userId);

                if (user.Roles.Any(userRole => blockedRoles.Select(x => x.Id).Contains(userRole.Id)))
                    return;
            }

            var gainedExp = new SafeRandom().Next(MinRandomExp, MaxRandomExp);
            await ProcessExpChangeAsync(client, guildId, channelId, userId, gainedExp, updateLastAwarded: true);
        }

        public async Task AddExpAsync(DiscordClient client, ulong guildId, ulong channelId, ulong userId, int exp)
        {
            await ProcessExpChangeAsync(client, guildId, channelId, userId, exp);
        }

        public async Task RemoveExpAsync(DiscordClient client, ulong guildId, ulong channelId, ulong userId, int exp)
        {
            await ProcessExpChangeAsync(client, guildId, channelId, userId, -exp);
        }

        private async Task ProcessExpChangeAsync(DiscordClient client, ulong guildId, ulong channelId, ulong userId, int expChange, bool updateLastAwarded = false)
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
                await _guildRoleService.UpdateLevelRolesAsync(client, guildId, channelId, userId, oldLevel, newLevel);
        }

        public async Task AddDuelWinAsync(ulong guildId, ulong userId)
        {
            await ModifyUserStatsAsync(guildId, userId, userStats => userStats.DuelWin++);
        }

        public async Task AddDuelLoseAsync(ulong guildId, ulong userId)
        {
            await ModifyUserStatsAsync(guildId, userId, userStats => userStats.DuelLose++);
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
                    LastExpAwardedAt = DateTime.MinValue
                };
                context.GuildUserStats.Add(userStats);
            }

            return userStats;
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

                if (statsToUpdate.Any())
                {
                    context.GuildUserStats.UpdateRange(statsToUpdate);
                }

                if (statsToAdd.Any())
                {
                    await context.GuildUserStats.AddRangeAsync(statsToAdd);
                }

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

    }
}
