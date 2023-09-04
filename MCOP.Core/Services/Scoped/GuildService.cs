using DSharpPlus.Entities;
using MCOP.Core.Common;
using MCOP.Core.Exceptions;
using MCOP.Core.Services.Shared.Common;
using MCOP.Data;
using MCOP.Data.Models;
using MCOP.Utils.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace MCOP.Core.Services.Scoped
{
    public class GuildService : IScoped
    {
        private readonly McopDbContext _context;

        public GuildService(McopDbContext context)
        {
            _context = context;
        }
        public async Task<Guild> GetOrAddGuildAsync(ulong guildId)
        {
            try
            {
                Guild? guild = await _context.Guilds.FindAsync(guildId);
                if (guild is null)
                {
                    guild = (await _context.Guilds.AddAsync(new Guild { Id = guildId })).Entity;
                    await _context.SaveChangesAsync();
                }

                return guild;
            }
            catch (Exception ex)
            {
                throw new McopException(ex, ex.Message);
            }
        }

        public async Task<GuildConfig> GetOrAddGuildConfigAsync(ulong guildId)
        {
            try
            {
                GuildConfig? config = await _context.GuildConfigs.FindAsync(guildId);
                if (config is null)
                {
                    var guild = await GetOrAddGuildAsync(guildId);
                    config = (await _context.GuildConfigs.AddAsync(new GuildConfig { Guild = guild })).Entity;
                    await _context.SaveChangesAsync();
                }

                return config;
            }
            catch (Exception ex)
            {
                throw new McopException(ex, ex.Message);
            }
        }

        public async Task<List<GuildConfig>> GetGuildConfigsWithLewdChannelAsync()
        {
            try
            {
                return await _context.GuildConfigs.Where(x => x.LewdChannelId != null).ToListAsync();
            }
            catch (Exception ex)
            {
                throw new McopException(ex, ex.Message);
            }
        }

        public async Task SetLewdChannelAsync(ulong guildId, ulong channelId)
        {
            try
            {
                GuildConfig config = await GetOrAddGuildConfigAsync(guildId);
                config.LewdChannelId = channelId;

                _context.GuildConfigs.Update(config);

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new McopException(ex, ex.Message);
            }
        }

        public async Task SetLoggingChannelAsync(ulong guildId, ulong channelId)
        {
            try
            {
                GuildConfig config = await GetOrAddGuildConfigAsync(guildId);
                config.LogChannelId = channelId;

                _context.GuildConfigs.Update(config);

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new McopException(ex, ex.Message);
            }
        }
    }
}
