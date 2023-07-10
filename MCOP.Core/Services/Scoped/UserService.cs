using MCOP.Core.Exceptions;
using MCOP.Data;
using MCOP.Data.Models;
using MCOP.Utils.Interfaces;

namespace MCOP.Core.Services.Scoped
{
    public class UserService : IScoped
    {
        private readonly McopDbContext _context;

        public UserService(McopDbContext context)
        {
            _context = context;
        }

        public async Task<GuildUser> GetOrAddUserAsync(ulong guildId, ulong userId)
        {
            try
            {
                var guild = await _context.Guilds.FindAsync(guildId);
                if (guild is null)
                {
                    guild = (await _context.Guilds.AddAsync(new Guild { Id = guildId })).Entity;
                    await _context.SaveChangesAsync();
                }
                var user = await _context.Users.FindAsync(userId);
                if (user is null)
                {
                    user = (await _context.Users.AddAsync(new User { Id = userId })).Entity;
                    await _context.SaveChangesAsync();
                }
                var guildUser = await _context.GuildUsers.FindAsync(guildId, userId);
                if (guildUser is null)
                {
                    guildUser = (await _context.GuildUsers.AddAsync(new GuildUser { GuildId = guildId, UserId = user.Id })).Entity;
                    await _context.SaveChangesAsync();
                }

                return guildUser;
            }
            catch (Exception ex)
            {
                throw new McopException(ex, ex.Message);
            }
        }
    }
}
