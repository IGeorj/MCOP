using MCOP.Data;
using MCOP.Data.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace MCOP.Core.Services.Scoped
{
    public interface IGuildMessageService
    {
        public Task RemoveMessageAsync(ulong guildId, ulong messageId);
    }

    public sealed class GuildMessageService : IGuildMessageService
    {
        private readonly IDbContextFactory<McopDbContext> _contextFactory;

        public GuildMessageService(IDbContextFactory<McopDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task RemoveMessageAsync(ulong guildId, ulong messageId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            try
            {
                var message = await context.GuildMessages.FindAsync(guildId, messageId);
                if (message is not null)
                {
                    var removedCount = await context.ImageHashes.CountAsync(x => x.GuildId == guildId && x.MessageId == messageId);
                    context.GuildMessages.Remove(message);
                    await context.SaveChangesAsync();
                    var totalCount = await context.ImageHashes.Where(x => x.GuildId == guildId).CountAsync();
                    Log.Information("Removed {Amount} hashes ({Total} total)", removedCount, totalCount);
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
