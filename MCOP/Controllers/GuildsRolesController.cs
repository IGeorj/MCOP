using Microsoft.AspNetCore.Mvc;
using DSharpPlus;
using MCOP.Core.Services.Scoped;
using MCOP.Common.Helpers;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/guilds")]
public class GuildsRolesController : ControllerBase
{
    private readonly DiscordClient _discordClient;
    private readonly IGuildRoleService _guildRoleService;
    private readonly Serilog.ILogger _logger;

    public GuildsRolesController(
        DiscordClient discordClient,
        IGuildRoleService guildRoleService,
        Serilog.ILogger logger)
    {
        _discordClient = discordClient;
        _guildRoleService = guildRoleService;
        _logger = logger;
    }

    [Authorize]
    [HttpPost("{guildId}/level-roles")]
    public async Task<IActionResult> AddOrUpdateLevelRole(
        string guildId,
        [FromBody] LevelRoleRequest request)
    {
        try
        {
            if (!ulong.TryParse(guildId, out var guildUlongId))
                return BadRequest("Invalid guild ID");

            if (!ulong.TryParse(request.RoleId, out var roleUlongId))
                return BadRequest("Invalid role ID");

            var forbiddenResult = await CheckUserPermissionsAsync(guildUlongId);
            if (forbiddenResult is not null)
                return forbiddenResult;

            await _guildRoleService.SetRoleLevelAsync(
                guildUlongId,
                roleUlongId,
                request.Level);

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error updating level role for guild {GuildId}", guildId);
            return StatusCode(500, "Failed to update level role");
        }
    }

    [Authorize]
    [HttpPost("{guildId}/level-roles/{roleId}/toggle-exp-block")]
    public async Task<IActionResult> ToggleExpBlock(
        string guildId,
        string roleId)
    {
        try
        {
            if (!ulong.TryParse(guildId, out var guildUlongId))
                return BadRequest("Invalid guild ID");

            if (!ulong.TryParse(roleId, out var roleUlongId))
                return BadRequest("Invalid role ID");

            var forbiddenResult = await CheckUserPermissionsAsync(guildUlongId);
            if (forbiddenResult is not null)
                return forbiddenResult;

            await _guildRoleService.ToggleBlockedRoleAsync(
                guildUlongId,
                roleUlongId);

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error toggling exp block for role {RoleId} in guild {GuildId}", roleId, guildId);
            return StatusCode(500, "Failed to toggle exp block");
        }
    }

    [Authorize]
    [HttpDelete("{guildId}/level-roles/{roleId}")]
    public async Task<IActionResult> RemoveLevelRole(
        string guildId,
        string roleId)
    {
        try
        {
            if (!ulong.TryParse(guildId, out var guildUlongId))
                return BadRequest("Invalid guild ID");

            if (!ulong.TryParse(roleId, out var roleUlongId))
                return BadRequest("Invalid role ID");

            var forbiddenResult = await CheckUserPermissionsAsync(guildUlongId);
            if (forbiddenResult is not null)
                return forbiddenResult;

            await _guildRoleService.SetRoleLevelAsync(
                guildUlongId,
                roleUlongId);

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error removing level role {RoleId} from guild {GuildId}", roleId, guildId);
            return StatusCode(500, "Failed to remove level role");
        }
    }

    private async Task<IActionResult?> CheckUserPermissionsAsync(ulong guildUlongId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized();

        var guild = await _discordClient.GetGuildAsync(guildUlongId);
        var user = await guild.GetMemberAsync(ulong.Parse(userId));

        var hasPermission = PermissionsHelper.HasManageServerPermission(user.Permissions.ToString());

        if (!hasPermission)
            return Forbid();

        return null;
    }

}

public class LevelRoleRequest
{
    public string RoleId { get; set; }
    public int Level { get; set; }
}