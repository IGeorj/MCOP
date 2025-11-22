using MCOP.Data;
using MCOP.Data.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace MCOP.Core.Services.Scoped
{
    public interface IImageVerificationChannelService
    {
        Task<bool> IsVerificationChannelAsync(ulong guildId, ulong channelId);
        Task<List<ulong>> GetImageVerificationChannelIdsAsync(ulong guildId);
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

        public async Task<ImageVerificationChannel?> GetVerificationChannelAsync(ulong guildId, ulong channelId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            return await context.ImageVerificationChannels
                .FirstOrDefaultAsync(c => c.GuildId == guildId && c.ChannelId == channelId);
        }

        public async Task<bool> IsVerificationChannelAsync(ulong guildId, ulong channelId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            return await context.ImageVerificationChannels
                .AnyAsync(c => c.GuildId == guildId && c.ChannelId == channelId);
        }
    
        public async Task<List<ulong>> GetImageVerificationChannelIdsAsync(ulong guildId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            try
            {
                var channels = await context.ImageVerificationChannels
                    .Where(ivc => ivc.GuildId == guildId)
                    .Select(ivc => ivc.ChannelId)
                    .ToListAsync();

                Log.Information("GetImageVerificationChannelIdsAsync guildId: {guildId}", guildId);

                return channels;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in GetImageVerificationChannelIdsAsync guildId: {guildId}", guildId);
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