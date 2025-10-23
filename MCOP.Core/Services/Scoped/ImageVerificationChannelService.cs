using MCOP.Data;
using MCOP.Data.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace MCOP.Core.Services.Scoped
{
    public interface IImageVerificationChannelService
    {
        Task AddVerificationChannelAsync(ulong guildId, ulong channelId);
        Task RemoveVerificationChannelAsync(ulong guildId, ulong channelId);
        Task<List<ulong>> GetVerificationChannelsAsync(ulong guildId);
        Task<bool> IsVerificationChannelAsync(ulong guildId, ulong channelId);
        Task<List<ulong>> GetImageVerificationChannelsAsync(ulong guildId);
        Task AddImageVerificationChannelAsync(ulong guildId, ulong channelId);
        Task RemoveImageVerificationChannelAsync(ulong guildId, ulong channelId);
    }

    public class ImageVerificationChannelService : IImageVerificationChannelService
    {
        private readonly IDbContextFactory<McopDbContext> _contextFactory;

        public ImageVerificationChannelService(IDbContextFactory<McopDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }
    
        public async Task<List<ulong>> GetVerificationChannelsAsync(ulong guildId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            return await context.ImageVerificationChannels
                .Where(c => c.GuildId == guildId)
                .Select(c => c.ChannelId)
                .ToListAsync();
        }
    
        public async Task<ImageVerificationChannel?> GetVerificationChannelAsync(ulong guildId, ulong channelId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            return await context.ImageVerificationChannels
                .FirstOrDefaultAsync(c => c.GuildId == guildId && c.ChannelId == channelId);
        }
    
        public async Task AddVerificationChannelAsync(ulong guildId, ulong channelId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var channel = new ImageVerificationChannel
            {
                GuildId = guildId,
                ChannelId = channelId
            };

            context.ImageVerificationChannels.Add(channel);

            await context.SaveChangesAsync();
        }
    
        public async Task RemoveVerificationChannelAsync(ulong guildId, ulong channelId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var channel = await context.ImageVerificationChannels
                .FirstOrDefaultAsync(c => c.GuildId == guildId && c.ChannelId == channelId);

            if (channel != null)
            {
                context.ImageVerificationChannels.Remove(channel);
                await context.SaveChangesAsync();
            }
        }
    
        public async Task<bool> IsVerificationChannelAsync(ulong guildId, ulong channelId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            return await context.ImageVerificationChannels
                .AnyAsync(c => c.GuildId == guildId && c.ChannelId == channelId);
        }
    
        public async Task<List<ulong>> GetImageVerificationChannelsAsync(ulong guildId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            try
            {
                var channels = await context.ImageVerificationChannels
                    .Where(ivc => ivc.GuildId == guildId)
                    .Select(ivc => ivc.ChannelId)
                    .ToListAsync();
                Log.Information("GetImageVerificationChannelsAsync guildId: {guildId}", guildId);
                return channels;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in GetImageVerificationChannelsAsync guildId: {guildId}", guildId);
                throw;
            }
        }
    
        public async Task AddImageVerificationChannelAsync(ulong guildId, ulong channelId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            try
            {
                var imageVerificationChannel = new ImageVerificationChannel
                {
                    GuildId = guildId,
                    ChannelId = channelId
                };

                context.ImageVerificationChannels.Add(imageVerificationChannel);

                await context.SaveChangesAsync();

                Log.Information("AddImageVerificationChannelAsync guildId: {guildId}, channelId: {channelId}", guildId, channelId);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in AddImageVerificationChannelAsync guildId: {guildId}, channelId: {channelId}", guildId, channelId);
                throw;
            }
        }
    
        public async Task RemoveImageVerificationChannelAsync(ulong guildId, ulong channelId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            try
            {
                var imageVerificationChannel = await context.ImageVerificationChannels
                    .FirstOrDefaultAsync(ivc => ivc.GuildId == guildId && ivc.ChannelId == channelId);
                if (imageVerificationChannel != null)
                {
                    context.ImageVerificationChannels.Remove(imageVerificationChannel);
                    await context.SaveChangesAsync();
                    Log.Information("RemoveImageVerificationChannelAsync guildId: {guildId}, channelId: {channelId}", guildId, channelId);
                }
                else
                {
                    Log.Warning("RemoveImageVerificationChannelAsync guildId: {guildId}, channelId: {channelId} - Channel not found", guildId, channelId);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in RemoveImageVerificationChannelAsync guildId: {guildId}, channelId: {channelId}", guildId, channelId);
                throw;
            }
        }
    }
}