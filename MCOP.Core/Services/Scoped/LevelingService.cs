using MCOP.Core.Common;
using MCOP.Data;
using MCOP.Data.Models;
using MCOP.Utils.Interfaces;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace MCOP.Core.Services.Scoped
{
    public class LevelingService : IScoped
    {

        private readonly McopDbContext _context;
        private readonly UserStatsService _userStatsService;

        public LevelingService(McopDbContext context, UserStatsService userStatsService, UserService userService)
        {
            _context = context;
            _userStatsService = userStatsService;
        }

        public async Task OnUserMessageCreatedAsync(ulong guildId, ulong userId)
        {
            try
            {
                string cacheKey = _userStatsService.GetCacheKey(guildId, userId);

                var lockKey = $"{cacheKey}_lock";
                using (var lockHandle = await SemaphoreLock.LockAsync(lockKey))
                {
                    GuildUserStat userStats = await _userStatsService.GetOrAddAsync(guildId, userId, isLocked: true);

                    DateTime now = DateTime.UtcNow;
                    TimeSpan timeSinceLastExp = now - userStats.LastExpAwardedAt;

                    if (timeSinceLastExp.TotalMinutes < 1)
                    {
                        return;
                    }

                    SafeRandom rng = new();
                    var gainedExp = rng.Next(15, 26);
                    userStats.Exp += gainedExp;
                    userStats.LastExpAwardedAt = now;

                    _context.GuildUserStats.Update(userStats);
                    await _context.SaveChangesAsync();

                    Log.Information("Exp added userId: {userId} amount: {exp}", userId, gainedExp);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in OnUserMessageCreatedAsync for userId: {userId}", userId);
            }
        }

        public async Task SetUsersExperienceAsync(ulong guildId, Dictionary<ulong, int> userIdExp)
        {
            var userIds = userIdExp.Keys.ToList();

            var existingUsers = await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .ToListAsync();

            var existingGuildUsers = await _context.GuildUsers
                .Where(gu => gu.GuildId == guildId && userIds.Contains(gu.UserId))
                .ToListAsync();

            var existingUsersDict = existingUsers.ToDictionary(u => u.Id);
            var existingGuildUsersDict = existingGuildUsers.ToDictionary(gu => gu.UserId);

            var usersToAdd = new List<User>();
            var guildUsersToAdd = new List<GuildUser>();
            var guildUserStatsToUpdate = new List<GuildUserStat>();
            var guildUserStatsToAdd = new List<GuildUserStat>();

            foreach (var userExp in userIdExp)
            {
                var userId = userExp.Key;
                var exp = userExp.Value;

                if (!existingUsersDict.ContainsKey(userId))
                {
                    var newUser = new User { Id = userId };
                    usersToAdd.Add(newUser);
                    existingUsersDict[userId] = newUser;
                }

                if (!existingGuildUsersDict.ContainsKey(userId))
                {
                    var newGuildUser = new GuildUser
                    {
                        GuildId = guildId,
                        UserId = userId
                    };
                    guildUsersToAdd.Add(newGuildUser);
                    existingGuildUsersDict[userId] = newGuildUser;
                }

                var guildUserStat = await _context.GuildUserStats.FindAsync(guildId, userId);
                if (guildUserStat is null)
                {
                    guildUserStatsToAdd.Add(new GuildUserStat
                    {
                        GuildId = guildId,
                        UserId = userId,
                        Exp = exp,
                        LastExpAwardedAt = DateTime.UtcNow
                    });
                }
                else
                {
                    guildUserStat.Exp = exp;
                    guildUserStatsToUpdate.Add(guildUserStat);
                }
            }

            if (usersToAdd.Any())
            {
                await _context.Users.AddRangeAsync(usersToAdd);
            }

            if (guildUsersToAdd.Any())
            {
                await _context.GuildUsers.AddRangeAsync(guildUsersToAdd);
            }

            if (guildUserStatsToUpdate.Any())
            {
                _context.GuildUserStats.UpdateRange(guildUserStatsToUpdate);
            }

            if (guildUserStatsToAdd.Any())
            {
                await _context.GuildUserStats.AddRangeAsync(guildUserStatsToAdd);
            }

            await _context.SaveChangesAsync();
        }
    }
}
