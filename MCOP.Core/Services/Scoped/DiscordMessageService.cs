using DSharpPlus.Entities;
using MCOP.Data;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace MCOP.Core.Services.Scoped
{
    public interface IDiscordMessageService
    {
        Task SendRoleGrantedMessageAsync(ulong guildId, DiscordMember user, DiscordRole role, int newLevel, DiscordChannel channel);
        Task SendRoleRevokedMessageAsync(ulong guildId, DiscordMember user, DiscordRole role, DiscordChannel channel);
        Task<bool> IsLevelUpMessagesEnabledAsync(ulong guildId);
        Task<string> BuildLevelUpMessageAsync(ulong guildId, DiscordMember user, DiscordRole role, int newLevel);
    }

    public class DiscordMessageService : IDiscordMessageService
    {
        private readonly IDbContextFactory<McopDbContext> _contextFactory;
        private readonly IGuildConfigService _guildConfigService;

        public DiscordMessageService(IDbContextFactory<McopDbContext> contextFactory, IGuildConfigService guildConfigService)
        {
            _contextFactory = contextFactory;
            _guildConfigService = guildConfigService;
        }

        public async Task SendRoleGrantedMessageAsync(ulong guildId, DiscordMember user, DiscordRole role, int newLevel, DiscordChannel channel)
        {
            if (!await IsLevelUpMessagesEnabledAsync(guildId))
                return;

            string content = await BuildLevelUpMessageAsync(guildId, user, role, newLevel);
            if (!string.IsNullOrWhiteSpace(content))
            {
                var messageBuilder = new DiscordMessageBuilder().WithContent(content);
                await messageBuilder.SendAsync(channel);
            }
        }

        public async Task SendRoleRevokedMessageAsync(ulong guildId, DiscordMember user, DiscordRole role, DiscordChannel channel)
        {
            if (!await IsLevelUpMessagesEnabledAsync(guildId))
                return;

            string content = $"<@{user.Id}> Заскамили на роль, убираем <@&{role.Id}>";
            var messageBuilder = new DiscordMessageBuilder().WithContent(content);
            await messageBuilder.SendAsync(channel);
        }

        public Task<bool> IsLevelUpMessagesEnabledAsync(ulong guildId)
            => _guildConfigService.IsLevelUpMessagesEnabledAsync(guildId);

        public async Task<string> BuildLevelUpMessageAsync(ulong guildId, DiscordMember user, DiscordRole role, int newLevel)
        {
            try
            {
                await using var context = _contextFactory.CreateDbContext();
                var config = await context.GuildConfigs.FindAsync(guildId);
                var roleEntity = await context.GuildRoles.FirstOrDefaultAsync(r => r.GuildId == guildId && r.Id == role.Id);
                var template = !string.IsNullOrWhiteSpace(roleEntity?.LevelUpMessageTemplate)
                    ? roleEntity.LevelUpMessageTemplate
                    : config?.LevelUpMessageTemplate;

                if (string.IsNullOrWhiteSpace(template))
                    return string.Empty;

                return template
                    .Replace("{user}", $"<@{user.Id}>")
                    .Replace("{role}", $"<@&{role.Id}>")
                    .Replace("{level}", newLevel.ToString());
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error building level-up message for guild {GuildId}", guildId);
                return string.Empty;
            }
        }
    }
}
