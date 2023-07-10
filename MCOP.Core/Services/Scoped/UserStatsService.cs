using MCOP.Core.Exceptions;
using MCOP.Core.ViewModels;
using MCOP.Data;
using MCOP.Data.Models;
using MCOP.Utils.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MCOP.Core.Services.Scoped
{
    public class UserStatsService : IScoped
    {
        private readonly McopDbContext _context;

        public UserStatsService(McopDbContext context)
        {
            _context = context;
        }

        public async Task<GuildUserStat> GetOrAddAsync(ulong guildId, ulong userId)
        {
            try
            {
                GuildUserStat? userStats = await _context.GuildUserStats.FindAsync(guildId, userId);
                if (userStats is null)
                {
                    userStats = (await _context.GuildUserStats.AddAsync(new GuildUserStat() { GuildId = guildId, UserId = userId })).Entity;
                    _context.SaveChanges();
                }
                return userStats;
            }
            catch (Exception ex)
            {
                throw new McopException(ex, ex.Message);
            }
        }

        public async Task<bool> ChangeLikeAsync(ulong guildId, ulong userId, ulong messageId, int count)
        {
            try
            {
                GuildUserStat userStats = await GetOrAddAsync(guildId, userId);
                userStats.Likes += count;

                _context.GuildUserStats.Update(userStats);
                await _context.SaveChangesAsync();
                var message = await _context.GuildMessages.FindAsync(guildId, messageId);
                if (message is null)
                {
                    message = (await _context.GuildMessages.AddAsync(new GuildMessage()
                    {
                        GuildId = guildId,
                        Id = messageId,
                        UserId = userId,
                    }))
                    .Entity;
                }
                message.Likes += count;
                _context.GuildUserStats.Update(userStats);
                await _context.SaveChangesAsync();
                return true;
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
                GuildUserStat userStats = await GetOrAddAsync(guildId, userId);
                userStats.DuelWin += count;

                _context.GuildUserStats.Update(userStats);
                await _context.SaveChangesAsync();
                return true;
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
                GuildUserStat userStats = await GetOrAddAsync(guildId, userId);
                userStats.DuelLose += count;

                _context.GuildUserStats.Update(userStats);
                await _context.SaveChangesAsync();
                return true;
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
                var topByLikes = await _context.GuildUserStats.OrderByDescending(x => x.Likes).Take(5).ToListAsync();
                var topDuels = await _context.GuildUserStats.OrderByDescending(x => x.DuelWin).Take(5).ToListAsync();
                var honorableMention = await _context.GuildUserStats.Where(x => x.GuildId == guildId)
                    .OrderByDescending(u => u.DuelLose - u.DuelWin).FirstOrDefaultAsync();

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
    }
}
