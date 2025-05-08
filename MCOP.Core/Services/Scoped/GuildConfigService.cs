using MCOP.Data;
using MCOP.Data.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;
namespace MCOP.Core.Services.Scoped
{
    public interface IGuildConfigService
    {
        public Task<GuildConfig> GetOrAddGuildConfigAsync(ulong guildId);
        public Task<List<GuildConfig>> GetGuildConfigsWithLewdChannelAsync();
        public Task SetLewdChannelAsync(ulong guildId, ulong channelId);
        public Task SetLoggingChannelAsync(ulong guildId, ulong channelId);
    }

    public class GuildConfigService : IGuildConfigService
    {
        private readonly IDbContextFactory<McopDbContext> _contextFactory;

        public GuildConfigService(IDbContextFactory<McopDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<GuildConfig> GetOrAddGuildConfigAsync(ulong guildId)
        {
            try
            {
                await using var context = _contextFactory.CreateDbContext();

                GuildConfig? config = await context.GuildConfigs.FindAsync(guildId);
                if (config is null)
                {
                    config = (await context.GuildConfigs.AddAsync(new GuildConfig { GuildId = guildId })).Entity;
                    await context.SaveChangesAsync();
                }

                Log.Information("GetOrAddGuildConfigAsync guildId: {guildId}", guildId);

                return config;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in GetOrAddGuildConfigAsync guildId: {guildId}", guildId);
                throw;
            }
        }

        public async Task<List<GuildConfig>> GetGuildConfigsWithLewdChannelAsync()
        {
            try
            {
                await using var context = _contextFactory.CreateDbContext();

                Log.Information("GetGuildConfigsWithLewdChannelAsync");

                return await context.GuildConfigs.Where(x => x.LewdChannelId != null).ToListAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in GetGuildConfigsWithLewdChannelAsync");
                throw;
            }
        }

        public async Task SetLewdChannelAsync(ulong guildId, ulong channelId)
        {
            try
            {
                await using var context = _contextFactory.CreateDbContext();

                GuildConfig config = await GetOrAddGuildConfigAsync(guildId);
                config.LewdChannelId = channelId;

                context.GuildConfigs.Update(config);

                await context.SaveChangesAsync();

                Log.Information("SetLewdChannelAsync guildId: {guildId}, channelId: {channelId}", guildId, channelId);

            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in SetLewdChannelAsync for guildId: {guildId}, channelId: {channelId}", guildId, channelId);
                throw;
            }
        }

        public async Task SetLoggingChannelAsync(ulong guildId, ulong channelId)
        {
            try
            {
                await using var context = _contextFactory.CreateDbContext();

                GuildConfig config = await GetOrAddGuildConfigAsync(guildId);
                config.LogChannelId = channelId;

                context.GuildConfigs.Update(config);

                await context.SaveChangesAsync();

                Log.Information("SetLoggingChannelAsync guildId: {guildId}, channelId: {channelId}", guildId, channelId);

            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in SetLoggingChannelAsync for guildId: {guildId}, channelId: {channelId}", guildId, channelId);
                throw;
            }
        }
    }
}
