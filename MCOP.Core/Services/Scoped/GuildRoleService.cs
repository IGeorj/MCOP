using DSharpPlus;
using DSharpPlus.Entities;
using MCOP.Core.Models;
using MCOP.Data;
using MCOP.Data.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Data;

namespace MCOP.Core.Services.Scoped
{
    public interface IGuildRoleService
    {
        public Task<List<GuildRoleDto>> GetGuildRolesAsync(ulong guildId);
        public Task<List<GuildRoleDto>> GetBlockedExpGuildRolesAsync(ulong guildId);
        public Task SetBlockedRoleAsync(ulong guildId, ulong roleId, bool isBlocked);
        public Task ToggleBlockedRoleAsync(ulong guildId, ulong roleId);
        public Task SetRoleLevelAsync(ulong guildId, ulong roleId, int? level = null);
        public Task SetRoleLevelUpMessageTemplateAsync(ulong guildId, ulong roleId, string? template);
        public Task ApplyLevelingRolesAsync(ulong guildId, ulong channelId, ulong userId, int oldLevel, int newLevel);
    }

    public sealed class GuildRoleService : IGuildRoleService
    {
        private readonly IDbContextFactory<McopDbContext> _contextFactory;
        private readonly IGuildConfigService _guildConfigService;
        private readonly IRoleApplicationService _roleApplicationService;
        private readonly DiscordClient _discordClient;

        public GuildRoleService(IDbContextFactory<McopDbContext> contextFactory, IGuildConfigService guildConfigService, IRoleApplicationService roleApplicationService, DiscordClient discordClient)
        {
            _contextFactory = contextFactory;
            _guildConfigService = guildConfigService;
            _roleApplicationService = roleApplicationService;
            _discordClient = discordClient;
        }

        public async Task<List<GuildRoleDto>> GetGuildRolesAsync(ulong guildId)
        {
            try
            {
                await using var context = _contextFactory.CreateDbContext();
                return await context.GuildRoles
                    .AsNoTracking()
                    .Where(us => us.GuildId == guildId)
                    .OrderBy(us => us.LevelToGetRole)
                    .Select(r => new GuildRoleDto(r.GuildId, r.Id, r.LevelToGetRole, r.IsGainExpBlocked, r.LevelUpMessageTemplate))
                    .ToListAsync();
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Log.Error(ex, "Error in GetGuildRoles for guildId: {guildId}", guildId);
                throw;
            }
        }

        public async Task<List<GuildRoleDto>> GetBlockedExpGuildRolesAsync(ulong guildId)
        {
            try
            {
                await using var context = _contextFactory.CreateDbContext();
                return await context.GuildRoles
                    .AsNoTracking()
                    .Where(us => us.GuildId == guildId && us.IsGainExpBlocked)
                    .Select(r => new GuildRoleDto(r.GuildId, r.Id, r.LevelToGetRole, r.IsGainExpBlocked, r.LevelUpMessageTemplate))
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

                Log.Information("ToggleBlockedRoleAsync: {guildId}, roleId: {roleId}, isBlocked: {isBlocked}", guildId, roleId, guildRole.IsGainExpBlocked);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Log.Error(ex, "Error in ToggleBlockedRoleAsync for guildId: {guildId}, roleId: {roleId}", guildId, roleId);
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

        public async Task SetRoleLevelUpMessageTemplateAsync(ulong guildId, ulong roleId, string? template)
        {
            try
            {
                await using var context = _contextFactory.CreateDbContext();

                var guildRole = await GetOrCreateGuildRoleInternalAsync(context, guildId, roleId);
                guildRole.LevelUpMessageTemplate = template;

                await context.SaveChangesAsync();

                Log.Information("SetRoleLevelUpMessageTemplateAsync: {guildId}, roleId: {roleId}, hasTemplate: {hasTemplate}", guildId, roleId, !string.IsNullOrWhiteSpace(template));
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Log.Error(ex, "Error in SetRoleLevelUpMessageTemplateAsync for guildId: {guildId}, roleId: {roleId}", guildId, roleId);
                throw;
            }
        }

        public async Task ApplyLevelingRolesAsync(ulong guildId, ulong channelId, ulong userId, int oldLevel, int newLevel)
        {
            if (oldLevel == newLevel) return;

            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();

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

                await _roleApplicationService.ApplyLevelingRolesAsync(guildId, channelId, userId, rolesToProcess, newLevel);

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
            var role = await context.GuildRoles.FirstOrDefaultAsync(r => r.GuildId == guildId && r.Id == roleId);
            if (role is null)
            {
                role = new GuildRole { GuildId = guildId, Id = roleId };
                context.GuildRoles.Add(role);
            }
            return role;
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
    }
}
