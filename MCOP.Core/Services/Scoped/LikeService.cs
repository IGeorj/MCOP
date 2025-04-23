using MCOP.Utils.Interfaces;
using Serilog;

namespace MCOP.Core.Services.Scoped
{
    public class LikeService : IScoped
    {
        private readonly GuildMessageService _guildMessageService;
        private readonly GuildUserStatsService _guildUserStatService;

        public LikeService(
            GuildMessageService guildMessageService,
            GuildUserStatsService guildUserStatService)
        {
            _guildMessageService = guildMessageService;
            _guildUserStatService = guildUserStatService;
        }

        public async Task AddLikeAsync(ulong guildId, ulong userId, ulong messageId)
        {
            try
            {
                await _guildMessageService.AddLikeAsync(guildId, userId, messageId);
                await _guildUserStatService.AddLikeAsync(guildId, userId);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in AddLikeAsync for guildId: {guildId}, userId: {userId}, messageId: {messageId}", guildId, userId, messageId);
                throw;
            }
        }

        public async Task RemoveLikeAsync(ulong guildId, ulong userId, ulong messageId)
        {
            try
            {
                await _guildMessageService.RemoveLikeAsync(guildId, userId, messageId);
                await _guildUserStatService.RemoveLikeAsync(guildId, userId);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in RemoveLikeAsync for guildId: {guildId}, userId: {userId}, messageId: {messageId}", guildId, userId, messageId);
                throw;
            }

        }
    }
}
