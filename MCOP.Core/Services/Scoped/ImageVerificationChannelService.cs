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
        private readonly McopDbContext _dbContext;
    
        public ImageVerificationChannelService(McopDbContext dbContext)
        {
            _dbContext = dbContext;
        }
    
        public async Task<List<ulong>> GetVerificationChannelsAsync(ulong guildId)
        {
            return await _dbContext.ImageVerificationChannels
                .Where(c => c.GuildId == guildId)
                .Select(c => c.ChannelId)
                .ToListAsync();
        }
    
        public async Task<ImageVerificationChannel?> GetVerificationChannelAsync(ulong guildId, ulong channelId)
        {
            return await _dbContext.ImageVerificationChannels
                .FirstOrDefaultAsync(c => c.GuildId == guildId && c.ChannelId == channelId);
        }
    
        public async Task AddVerificationChannelAsync(ulong guildId, ulong channelId)
        {
            var channel = new ImageVerificationChannel
            {
                GuildId = guildId,
                ChannelId = channelId
            };
            _dbContext.ImageVerificationChannels.Add(channel);
            await _dbContext.SaveChangesAsync();
        }
    
        public async Task RemoveVerificationChannelAsync(ulong guildId, ulong channelId)
        {
            var channel = await _dbContext.ImageVerificationChannels
                .FirstOrDefaultAsync(c => c.GuildId == guildId && c.ChannelId == channelId);
            if (channel != null)
            {
                _dbContext.ImageVerificationChannels.Remove(channel);
                await _dbContext.SaveChangesAsync();
            }
        }
    
        public async Task<bool> IsVerificationChannelAsync(ulong guildId, ulong channelId)
        {
            return await _dbContext.ImageVerificationChannels
                .AnyAsync(c => c.GuildId == guildId && c.ChannelId == channelId);
        }
    
        public async Task<List<ulong>> GetImageVerificationChannelsAsync(ulong guildId)
        {
            try
            {
                var channels = await _dbContext.ImageVerificationChannels
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
            try
            {
                var imageVerificationChannel = new ImageVerificationChannel
                {
                    GuildId = guildId,
                    ChannelId = channelId
                };
                _dbContext.ImageVerificationChannels.Add(imageVerificationChannel);
                await _dbContext.SaveChangesAsync();
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
            try
            {
                var imageVerificationChannel = await _dbContext.ImageVerificationChannels
                    .FirstOrDefaultAsync(ivc => ivc.GuildId == guildId && ivc.ChannelId == channelId);
                if (imageVerificationChannel != null)
                {
                    _dbContext.ImageVerificationChannels.Remove(imageVerificationChannel);
                    await _dbContext.SaveChangesAsync();
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