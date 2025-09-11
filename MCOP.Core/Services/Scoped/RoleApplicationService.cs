using DSharpPlus;
using DSharpPlus.Entities;
using MCOP.Data.Models;
using Microsoft.Extensions.Logging;

namespace MCOP.Core.Services.Scoped
{
    public interface IRoleApplicationService
    {
        Task ApplyLevelingRolesAsync(ulong guildId, ulong channelId, ulong userId, List<GuildRole> rolesToProcess, int newLevel);
    }

    public class RoleApplicationService : IRoleApplicationService
    {
        private readonly DiscordClient _discordClient;
        private readonly IDiscordMessageService _discordMessageService;
        private readonly ILogger<RoleApplicationService> _logger;

        public RoleApplicationService(DiscordClient discordClient, IDiscordMessageService discordMessageService, ILogger<RoleApplicationService> logger)
        {
            _discordClient = discordClient;
            _discordMessageService = discordMessageService;
            _logger = logger;
        }

        public async Task ApplyLevelingRolesAsync(ulong guildId, ulong channelId, ulong userId,
            List<GuildRole> rolesToProcess, int newLevel)
        {
            if (rolesToProcess.Count == 0)
                return;

            var guild = await _discordClient.GetGuildAsync(guildId);
            if (guild is null)
            {
                _logger.LogWarning("Guild {GuildId} not found", guildId);
                return;
            }

            var user = await guild.GetMemberAsync(userId);
            if (user is null)
            {
                _logger.LogWarning("User {UserId} not found in guild {GuildId}", userId, guildId);
                return;
            }

            var channel = await guild.GetChannelAsync(channelId);

            foreach (var role in rolesToProcess)
            {
                await ProcessLevelingRoleAsync(guild, user, channel, role, newLevel);
            }
        }

        private async Task ProcessLevelingRoleAsync(DiscordGuild guild, DiscordMember user,
            DiscordChannel? channel, GuildRole role, int newLevel)
        {
            var discordRole = await guild.GetRoleAsync(role.Id);
            if (discordRole is null)
            {
                _logger.LogWarning("Role {RoleId} not found in guild {GuildId}", role.Id, guild.Id);
                return;
            }

            try
            {
                if (role.LevelToGetRole <= newLevel)
                    await GrantRoleWithNotificationAsync(guild.Id, user, discordRole, channel, newLevel);
                else
                    await RevokeRoleWithNotificationAsync(guild.Id, user, discordRole, channel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to modify role {RoleId} for user {UserId}", role.Id, user.Id);
            }
        }

        private async Task GrantRoleWithNotificationAsync(ulong guildId, DiscordMember user,
            DiscordRole role, DiscordChannel? channel, int newLevel)
        {
            if (channel is not null)
                await _discordMessageService.SendRoleGrantedMessageAsync(guildId, user, role, newLevel, channel);

            await user.GrantRoleAsync(role, "LvlUp");
            _logger.LogInformation("Added role {RoleName} to user {UserId}", role.Name, user.Id);
        }

        private async Task RevokeRoleWithNotificationAsync(ulong guildId, DiscordMember user,
            DiscordRole role, DiscordChannel? channel)
        {
            if (channel is not null)
                await _discordMessageService.SendRoleRevokedMessageAsync(guildId, user, role, channel);

            await user.RevokeRoleAsync(role);
            _logger.LogInformation("Removed role {RoleName} from user {UserId}", role.Name, user.Id);
        }
    }
}
