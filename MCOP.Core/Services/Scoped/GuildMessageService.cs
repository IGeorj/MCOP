using MCOP.Data;
using MCOP.Data.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace MCOP.Core.Services.Scoped
{
    public interface IGuildMessageService
    {
        public Task AddLikeAsync(ulong guildId, ulong messageId, ulong userId);
        public Task RemoveLikeAsync(ulong guildId, ulong messageId, ulong userId);
        public Task RemoveMessageAsync(ulong guildId, ulong messageId);
    }

    public sealed class GuildMessageService : IGuildMessageService
    {
        private readonly IDbContextFactory<McopDbContext> _contextFactory;

        public GuildMessageService(IDbContextFactory<McopDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task AddLikeAsync(ulong guildId, ulong messageId, ulong userId)
        {
            try
            {
                await using var context = _contextFactory.CreateDbContext();

                var message = await GetOrCreateMessageInternalAsync(guildId, messageId, userId);
                message.Likes++;
                await context.SaveChangesAsync();

                Log.Information("AddLikeAsync guildId: {guildId}, messageId: {messageId}, userId: {userId}", guildId, messageId, userId);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in AddLikeAsync for guildId: {guildId}, messageId: {messageId}, userId: {userId}", guildId, messageId, userId);
            }

        }

        public async Task RemoveLikeAsync(ulong guildId, ulong messageId, ulong userId)
        {
            try
            {
                await using var context = _contextFactory.CreateDbContext();

                var message = await GetOrCreateMessageInternalAsync(guildId, messageId, userId);
                if (message.Likes > 0)
                    message.Likes--;

                await context.SaveChangesAsync();

                Log.Information("RemoveLikeAsync guildId: {guildId}, messageId: {messageId}, userId: {userId}", guildId, messageId, userId);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in RemoveLikeAsync for guildId: {guildId}, messageId: {messageId}, userId: {userId}", guildId, messageId, userId);
            }
        }

        private async Task<GuildMessage> GetOrCreateMessageInternalAsync(ulong guildId, ulong messageId, ulong userId)
        {
            await using var context = _contextFactory.CreateDbContext();

            var message = await context.GuildMessages
                .SingleOrDefaultAsync(m => m.GuildId == guildId && m.Id == messageId);
            if (message == null)
            {
                message = new GuildMessage
                {
                    GuildId = guildId,
                    Id = messageId,
                    UserId = userId,
                    Likes = 0
                };
                context.GuildMessages.Add(message);
            }
            return message;
        }

        public async Task RemoveMessageAsync(ulong guildId, ulong messageId)
        {
            await using var context = _contextFactory.CreateDbContext();

            try
            {
                var message = await context.GuildMessages.FindAsync(guildId, messageId);
                if (message is not null)
                {
                    var hashesCount = await context.ImageHashes.CountAsync(x => x.GuildId == guildId && x.MessageId == messageId);
                    context.GuildMessages.Remove(message);
                    await context.SaveChangesAsync();
                    var removedHashes = await context.ImageHashes.CountAsync();
                    Log.Information("Removed {Amount} hashes ({Total} total)", hashesCount, removedHashes);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in RemoveMessageAsync for guildId: {guildId}, messageId: {messageId}", guildId, messageId);
                throw;
            }
        }
    }
}
