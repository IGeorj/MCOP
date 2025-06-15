using DSharpPlus;
using DSharpPlus.Entities;
using MCOP.Data;
using MCOP.Data.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Data;

namespace MCOP.Core.Services.Scoped
{
    public interface IGuildRoleService
    {
        public Task<List<GuildRole>> GetGuildRolesAsync(ulong guildId);
        public Task<List<GuildRole>> GetBlockedExpGuildRolesAsync(ulong guildId);
        public Task SetBlockedRoleAsync(ulong guildId, ulong roleId, bool isBlocked);
        public Task ToggleBlockedRoleAsync(ulong guildId, ulong roleId);
        public Task SetRoleLevelAsync(ulong guildId, ulong roleId, int? level = null);
        public Task UpdateLevelRolesAsync(ulong guildId, ulong channelId, ulong userId, int oldLevel, int newLevel);
    }

    public class GuildRoleService : IGuildRoleService
    {
        public readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(5);

        private readonly IDbContextFactory<McopDbContext> _contextFactory;
        private readonly DiscordClient _discordClient;

        public GuildRoleService(IDbContextFactory<McopDbContext> contextFactory, DiscordClient discordClient)
        {
            _contextFactory = contextFactory;
            _discordClient = discordClient;
        }

        public async Task<List<GuildRole>> GetGuildRolesAsync(ulong guildId)
        {
            try
            {
                await using var context = _contextFactory.CreateDbContext();
                return await context.GuildRoles
                    .AsNoTracking()
                    .Where(us => us.GuildId == guildId)
                    .OrderBy(us => us.LevelToGetRole)
                    .ToListAsync();
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Log.Error(ex, "Error in GetGuildRoles for guildId: {guildId}", guildId);
                throw;
            }
        }

        public async Task<List<GuildRole>> GetBlockedExpGuildRolesAsync(ulong guildId)
        {
            try
            {
                await using var context = _contextFactory.CreateDbContext();
                return await context.GuildRoles
                    .AsNoTracking()
                    .Where(us => us.GuildId == guildId && us.IsGainExpBlocked)
                    .ToListAsync();
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Log.Error(ex, "Error in GetBlockedExpGuildRolesAsync for guildId: {guildId}", guildId);
                throw;
            }
        }

        public async Task SetBlockedRoleAsync(ulong guildId, ulong roleId, bool isBlocked)
        {
            try
            {
                await using var context = _contextFactory.CreateDbContext();

                var guildRole = await GetOrCreateGuildRoleInternalAsync(context, guildId, roleId);
                guildRole.IsGainExpBlocked = isBlocked;

                await context.SaveChangesAsync();

                Log.Information("SetBlockedRoleAsync: {guildId}, roleId: {roleId}, isBlocked: {isBlocked}", guildId, roleId, isBlocked);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Log.Error(ex, "Error in SetBlockedRoleAsync for guildId: {guildId}, roleId: {roleId}, isBlocked: {isBlocked}", guildId, roleId, isBlocked);
                throw;
            }
        }
        public async Task ToggleBlockedRoleAsync(ulong guildId, ulong roleId)
        {
            try
            {
                await using var context = _contextFactory.CreateDbContext();

                var guildRole = await GetOrCreateGuildRoleInternalAsync(context, guildId, roleId);
                guildRole.IsGainExpBlocked = !guildRole.IsGainExpBlocked;

                await context.SaveChangesAsync();

                Log.Information("ToggleBlockedRoleAsync: {guildId}, roleId: {roleId}, isBlocked: {isBlocked}", guildId, roleId);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Log.Error(ex, "Error in ToggleBlockedRoleAsync for guildId: {guildId}, roleId: {roleId}, isBlocked: {isBlocked}", guildId, roleId);
                throw;
            }
        }

        public async Task SetRoleLevelAsync(ulong guildId, ulong roleId, int? level)
        {
            try
            {
                await using var context = _contextFactory.CreateDbContext();

                var guildRole = await GetOrCreateGuildRoleInternalAsync(context, guildId, roleId);
                guildRole.LevelToGetRole = level;

                await context.SaveChangesAsync();

                Log.Information("SetRoleLevel: {guildId}, roleId: {roleId}, level: {level}", guildId, roleId, level);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Log.Error(ex, "Error in SetRoleLevel for guildId: {guildId}, roleId: {roleId}, level: {level}", guildId, roleId, level);
                throw;
            }
        }

        public async Task UpdateLevelRolesAsync(ulong guildId, ulong channelId, ulong userId, int oldLevel, int newLevel)
        {
            if (oldLevel == newLevel) return;

            try
            {
                await using var context = _contextFactory.CreateDbContext();

                var guildRoles = await context.GuildRoles
                    .Where(g => g.GuildId == guildId && g.LevelToGetRole != null)
                    .ToListAsync();

                if (guildRoles == null || guildRoles.Count == 0)
                {
                    Log.Debug("Guild roles not found for {GuildId}", guildId);
                    return;
                }

                var rolesToProcess = GetRolesToProcess(guildRoles, oldLevel, newLevel);
                if (rolesToProcess.Count == 0)
                {
                    Log.Debug("No role changes needed for user {UserId} in guild {GuildId}", userId, guildId);
                    return;
                }

                await ApplyRoleChangesAsync(guildId, channelId, userId, rolesToProcess, newLevel);

                foreach (var role in rolesToProcess)
                {
                    Log.Information("Updated roles for user {UserId} in guild {GuildId} (Level {OldLevel} → {NewLevel}), roleId: {RoleId}",
                        userId, guildId, oldLevel, newLevel, role.Id);
                }

            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Log.Error(ex, "Failed to update roles for user {UserId} in guild {GuildId}", userId, guildId);
                throw;
            }
        }

        private async Task<GuildRole> GetOrCreateGuildRoleInternalAsync(McopDbContext context, ulong guildId, ulong roleId)
        {
            var guildRole = await context.GuildRoles
                .SingleOrDefaultAsync(us => us.GuildId == guildId && us.Id == roleId);

            if (guildRole == null)
            {
                guildRole = new GuildRole
                {
                    GuildId = guildId,
                    Id = roleId,
                };
                context.GuildRoles.Add(guildRole);
            }

            return guildRole;
        }

        private List<GuildRole> GetRolesToProcess(List<GuildRole> levelRoles, int oldLevel, int newLevel)
        {
            var rolesToProcess = new List<GuildRole>();

            if (newLevel > oldLevel)
            {
                rolesToProcess.AddRange(levelRoles
                    .Where(r => r.LevelToGetRole > oldLevel && r.LevelToGetRole <= newLevel));
            }
            else
            {
                rolesToProcess.AddRange(levelRoles
                    .Where(r => r.LevelToGetRole > newLevel && r.LevelToGetRole <= oldLevel));
            }

            return rolesToProcess.Distinct().ToList();
        }

        private async Task ApplyRoleChangesAsync(ulong guildId, ulong channelId, ulong userId, List<GuildRole> rolesToProcess, int newLevel)
        {
            if (rolesToProcess.Count == 0)
                return;

            var guild = await _discordClient.GetGuildAsync(guildId);
            if (guild is null)
            {
                Log.Warning("Guild {GuildId} not found", guildId);
                return;
            }

            var user = await guild.GetMemberAsync(userId);
            if (user is null)
            {
                Log.Warning("User {UserId} not found in guild {GuildId}", userId, guildId);
                return;
            }

            var channel = await guild.GetChannelAsync(channelId);

            foreach (var role in rolesToProcess)
            {
                var discordRole = await guild.GetRoleAsync(role.Id);
                if (discordRole is null)
                {
                    Log.Warning("Role {RoleId} not found in guild {GuildId}", role.Id, guildId);
                    continue;
                }

                try
                {
                    if (role.LevelToGetRole <= newLevel)
                    {
                        if (channel is not null)
                        {
                            DiscordMessageBuilder messageBuilder = new DiscordMessageBuilder();
                            messageBuilder.AddMention(new UserMention(user));
                            messageBuilder.WithContent($"<@{user.Id}> Ого ты жёсткий, повышаем до <@&{role.Id}>");
                            await messageBuilder.SendAsync(channel);
                        }

                        await user.GrantRoleAsync(discordRole, "LvlUp");
                        Log.Information("Added role {RoleName} to user {UserId}", discordRole.Name, userId);
                    }
                    else
                    {
                        if (channel is not null)
                        {
                            DiscordMessageBuilder messageBuilder = new DiscordMessageBuilder();
                            messageBuilder.WithContent($"<@{user.Id}> Заскамили на роль, убираем <@&{role.Id}>");
                            await messageBuilder.SendAsync(channel);
                        }

                        await user.RevokeRoleAsync(discordRole);
                        Log.Information("Removed role {RoleName} from user {UserId}", discordRole.Name, userId);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Failed to modify role {RoleId} for user {UserId}", role.Id, userId);
                }
            }
        }
    }
}
