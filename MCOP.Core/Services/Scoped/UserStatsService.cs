using MCOP.Core.Common;
using MCOP.Core.Exceptions;
using MCOP.Core.ViewModels;
using MCOP.Data;
using MCOP.Data.Models;
using MCOP.Utils.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace MCOP.Core.Services.Scoped
{
    public class UserStatsService : IScoped
    {
        public readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(5);

        private readonly IMemoryCache _cache;
        private readonly McopDbContext _context;
        private readonly UserService _userService;

        public UserStatsService(IMemoryCache cache, McopDbContext context, UserService userService)
        {
            _cache = cache;
            _context = context;
            _userService = userService;
        }

        public async Task<GuildUserStat> GetOrAddAsync(ulong guildId, ulong userId, bool isLocked = false)
        {
            try
            {
                string cacheKey = GetCacheKey(guildId, userId);

                if (!isLocked)
                {
                    var lockKey = $"{cacheKey}_lock";
                    using (var lockHandle = await SemaphoreLock.LockAsync(lockKey))
                    {
                        return await GetOrAddAsyncInternal(guildId, userId, cacheKey);
                    }
                }
                else
                {
                    return await GetOrAddAsyncInternal(guildId, userId, cacheKey);
                }
            }
            catch (Exception ex)
            {
                throw new McopException(ex, ex.Message);
            }
        }
        private async Task<GuildUserStat> GetOrAddAsyncInternal(ulong guildId, ulong userId, string cacheKey)
        {
            if (_cache.TryGetValue(cacheKey, out GuildUserStat? cachedUserStat))
            {
                return cachedUserStat;
            }

            var userStats = await _context.GuildUserStats.FindAsync(guildId, userId);

            if (userStats is null)
            {
                await _userService.GetOrAddUserAsync(guildId, userId);

                userStats = new GuildUserStat { GuildId = guildId, UserId = userId };
                _context.GuildUserStats.Add(userStats);
                await _context.SaveChangesAsync();
            }

            //_cache.Set(cacheKey, userStats, CacheExpiration);

            return userStats;
        }

        public async Task<bool> ChangeLikeAsync(ulong guildId, ulong userId, ulong messageId, int count)
        {
            try
            {
                var cacheKey = GetCacheKey(guildId, userId);
                var lockKey = $"{cacheKey}_lock";

                using (var lockHandle = await SemaphoreLock.LockAsync(lockKey))
                {
                    GuildUserStat userStats = await GetOrAddAsync(guildId, userId, isLocked: true);
                    userStats.Likes += count;

                    var message = await _context.GuildMessages.FindAsync(guildId, messageId);
                    if (message is null)
                    {
                        message = (await _context.GuildMessages.AddAsync(new GuildMessage()
                        {
                            GuildId = guildId,
                            Id = messageId,
                            UserId = userId,
                            Likes = count
                        })).Entity;
                    }
                    else
                    {
                        message.Likes += count;
                    }

                    await _context.SaveChangesAsync();

                    _cache.Set(cacheKey, userStats, CacheExpiration);

                    return true;
                }
            }
            catch (Exception ex)
            {
                throw new McopException(ex, ex.Message);
            }
        }

        public async Task<bool> ChangeWinAsync(ulong guildId, ulong userId, int count)
        {
            try
            {
                var cacheKey = GetCacheKey(guildId, userId);
                var lockKey = $"{cacheKey}_lock";

                using (var lockHandle = await SemaphoreLock.LockAsync(lockKey))
                {
                    GuildUserStat userStats = await GetOrAddAsync(guildId, userId, isLocked: true);
                    userStats.DuelWin += count;

                    await _context.SaveChangesAsync();

                    _cache.Set(cacheKey, userStats, CacheExpiration);

                    return true;
                }
            }
            catch (Exception ex)
            {
                throw new McopException(ex, ex.Message);
            }
        }

        public async Task<bool> ChangeLoseAsync(ulong guildId, ulong userId, int count)
        {
            try
            {
                var cacheKey = GetCacheKey(guildId, userId);
                var lockKey = $"{cacheKey}_lock";

                using (var lockHandle = await SemaphoreLock.LockAsync(lockKey))
                {
                    GuildUserStat userStats = await GetOrAddAsync(guildId, userId, isLocked: true);
                    userStats.DuelLose += count;

                    await _context.SaveChangesAsync();

                    _cache.Set(cacheKey, userStats, CacheExpiration);

                    return true;
                }
            }
            catch (Exception ex)
            {
                throw new McopException(ex, ex.Message);
            }
        }

        public async Task<ServerTopVM> GetServerTopAsync(ulong guildId)
        {
            try
            {
                var topByLikes = await _context.GuildUserStats.OrderByDescending(x => x.Likes).Take(20).ToListAsync();
                var topDuels = await _context.GuildUserStats.OrderByDescending(x => x.DuelWin).Take(20).ToListAsync();
                var honorableMention = await _context.GuildUserStats.Where(x => x.GuildId == guildId)
                    .OrderByDescending(u => u.DuelLose - u.DuelWin).Take(20).ToListAsync();

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

        public string GetCacheKey(ulong guildId, ulong userId)
        {
            return $"GuildUserStat_{guildId}_{userId}";
        }
    }
}
