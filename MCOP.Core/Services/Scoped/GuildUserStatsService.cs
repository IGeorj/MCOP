using MCOP.Core.Common;
using MCOP.Core.Exceptions;
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
        public GuildUserStatsService(IDbContextFactory<McopDbContext> contextFactory, ILockingService lockingService)
        {
            _contextFactory = contextFactory;
            _lockingService = lockingService;
        }

        public async Task<ServerTopVM> GetServerTopAsync(ulong guildId)
        {
            try
            {
                await using var context = _contextFactory.CreateDbContext();

                var topByLikes = await context.GuildUserStats
                    .OrderByDescending(x => x.Likes)
                    .Take(20)
                    .ToListAsync();

                var topDuels = await context.GuildUserStats.
                    OrderByDescending(x => x.DuelWin)
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
                throw new McopException(ex, ex.Message);
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
                {
                    userStats.Likes--;
                }
            });
        }

        public async Task AddMessageExpAsync(ulong guildId, ulong userId)
        {
            await using var context = _contextFactory.CreateDbContext();

            var userStats = await context.GuildUserStats.SingleOrDefaultAsync(us => us.GuildId == guildId && us.UserId == userId);
            if (userStats is not null)
            {
                TimeSpan timeSinceLastExp = DateTime.UtcNow - userStats.LastExpAwardedAt;

                if (timeSinceLastExp.TotalMinutes < 1)
                {
                    return;
                }
            }

            SafeRandom rng = new();
            var gainedExp = rng.Next(15, 26);

            await ModifyUserStatsAsync(guildId, userId, userStats => {
                userStats.Exp += gainedExp;
                userStats.LastExpAwardedAt = DateTime.UtcNow;
            });
        }

        public async Task AddExpAsync(ulong guildId, ulong userId, int exp)
        {
            await ModifyUserStatsAsync(guildId, userId, userStats => userStats.Exp += exp);
        }

        public async Task RemoveExpAsync(ulong guildId, ulong userId, int exp)
        {
            await ModifyUserStatsAsync(guildId, userId, userStats =>
            {
                if (userStats.Exp >= exp)
                {
                    userStats.Exp -= exp;
                }
            });
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

                    var originalValues = GetPropertyValues(userStats);

                    modifyAction(userStats);

                    var updatedValues = GetPropertyValues(userStats);

                    LogChanges(originalValues, updatedValues, guildId, userId);

                    await context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error in ModifyUserStatAsync for guildId: {guildId}, userId: {userId}", guildId, userId);
                    throw new McopException(ex, ex.Message);
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
                throw new McopException(ex, ex.Message);
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
                throw new McopException(ex, ex.Message);
            }
        }

        private Dictionary<string, object?> GetPropertyValues(GuildUserStats userStats)
        {
            return typeof(GuildUserStats)
                .GetProperties()
                .Where(p => p.CanRead)
                .ToDictionary(p => p.Name, p => p.GetValue(userStats));
        }

        private void LogChanges(Dictionary<string, object?> originalValues, Dictionary<string, object?> updatedValues, ulong guildId, ulong userId)
        {
            var changes = new List<string>();

            foreach (var key in originalValues.Keys)
            {
                var originalValue = originalValues[key];
                var updatedValue = updatedValues[key];

                if (!Equals(originalValue, updatedValue))
                {
                    changes.Add($"{key}: {originalValue} -> {updatedValue}");
                }
            }

            if (changes.Count != 0)
            {
                Log.Information(
                    "ModifyUserStatsAsync: Changes detected for guildId: {guildId}, userId: {userId}. Changes:\n{changes}",
                    guildId, userId, string.Join(Environment.NewLine, changes));
            }
            else
            {
                Log.Information(
                    "ModifyUserStatsAsync: No changes detected for guildId: {guildId}, userId: {userId}",
                    guildId, userId);
            }
        }
    }
}
