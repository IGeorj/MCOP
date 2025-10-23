using MCOP.Core.Models;
using MCOP.Data;
using MCOP.Data.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;
namespace MCOP.Core.Services.Scoped
{
    public interface IGuildConfigService
    {
        public Task<GuildConfigDto> GetOrAddGuildConfigAsync(ulong guildId);
        public Task<List<GuildConfigDto>> GetGuildConfigsWithLewdChannelAsync();
        public Task SetLewdChannelAsync(ulong guildId, ulong? channelId);
        public Task SetLoggingChannelAsync(ulong guildId, ulong channelId);
        public Task SetLevelUpMessageTemplateAsync(ulong guildId, string? template);
        public Task<bool> IsLevelUpMessagesEnabledAsync(ulong guildId);
        public Task SetLevelUpMessagesEnabledAsync(ulong guildId, bool enabled);
    }

    public sealed class GuildConfigService : IGuildConfigService
    {
        private readonly IDbContextFactory<McopDbContext> _contextFactory;

        public GuildConfigService(IDbContextFactory<McopDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<GuildConfigDto> GetOrAddGuildConfigAsync(ulong guildId)
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();

                GuildConfig? config = await context.GuildConfigs.FindAsync(guildId);
                if (config is null)
                {
                    config = (await context.GuildConfigs.AddAsync(new GuildConfig { GuildId = guildId })).Entity;
                    await context.SaveChangesAsync();
                }

                Log.Information("GetOrAddGuildConfigAsync guildId: {guildId}", guildId);

                return new GuildConfigDto(config.GuildId, config.Prefix, config.LogChannelId, config.LoggingEnabled, config.LewdChannelId, config.LewdEnabled, config.LevelUpMessageTemplate, config.LevelUpMessagesEnabled);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in GetOrAddGuildConfigAsync guildId: {guildId}", guildId);
                throw;
            }
        }

        public async Task<List<GuildConfigDto>> GetGuildConfigsWithLewdChannelAsync()
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();

                Log.Information("GetGuildConfigsWithLewdChannelAsync");

                return await context.GuildConfigs
                    .AsNoTracking()
                    .Where(x => x.LewdChannelId != null)
                    .Select(c => new GuildConfigDto(c.GuildId, c.Prefix, c.LogChannelId, c.LoggingEnabled, c.LewdChannelId, c.LewdEnabled, c.LevelUpMessageTemplate, c.LevelUpMessagesEnabled))
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in GetGuildConfigsWithLewdChannelAsync");
                throw;
            }
        }

        public async Task SetLewdChannelAsync(ulong guildId, ulong? channelId)
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();

                GuildConfig config = await GetOrAddGuildConfigInternalAsync(guildId);
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
                await using var context = await _contextFactory.CreateDbContextAsync();

                GuildConfig config = await GetOrAddGuildConfigInternalAsync(guildId);
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

        public async Task<bool> IsLevelUpMessagesEnabledAsync(ulong guildId)
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                var config = await context.GuildConfigs.FindAsync(guildId);
                return config?.LevelUpMessagesEnabled ?? true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error checking LevelUpMessagesEnabled for guild {GuildId}", guildId);
                return true;
            }
        }

        public async Task SetLevelUpMessageTemplateAsync(ulong guildId, string? template)
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();

                GuildConfig config = await GetOrAddGuildConfigInternalAsync(guildId);
                config.LevelUpMessageTemplate = template;

                context.GuildConfigs.Update(config);
                await context.SaveChangesAsync();

                Log.Information("SetLevelUpMessageTemplateAsync guildId: {guildId}, hasTemplate: {hasTemplate}", guildId, !string.IsNullOrWhiteSpace(template));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in SetLevelUpMessageTemplateAsync for guildId: {guildId}", guildId);
                throw;
            }
        }

        public async Task SetLevelUpMessagesEnabledAsync(ulong guildId, bool enabled)
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();

                GuildConfig config = await GetOrAddGuildConfigInternalAsync(guildId);
                config.LevelUpMessagesEnabled = enabled;

                context.GuildConfigs.Update(config);
                await context.SaveChangesAsync();

                Log.Information("SetLevelUpMessagesEnabledAsync guildId: {guildId}, enabled: {enabled}", guildId, enabled);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in SetLevelUpMessagesEnabledAsync for guildId: {guildId}", guildId);
                throw;
            }
        }


        private async Task<GuildConfig> GetOrAddGuildConfigInternalAsync(ulong guildId)
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();

                GuildConfig? config = await context.GuildConfigs.FindAsync(guildId);
                if (config is null)
                {
                    config = (await context.GuildConfigs.AddAsync(new GuildConfig { GuildId = guildId })).Entity;
                    await context.SaveChangesAsync();
                }

                Log.Information("GetOrAddGuildConfigInternalAsync guildId: {guildId}", guildId);

                return config;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in GetOrAddGuildConfigInternalAsync guildId: {guildId}", guildId);
                throw;
            }
        }

    }
}
