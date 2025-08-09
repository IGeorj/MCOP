using Microsoft.AspNetCore.Mvc;
using DSharpPlus;
using MCOP.Common.Helpers;
using System.Security.Claims;

namespace MCOP.Controllers
{
    public class BasePermissionController : ControllerBase
    {
        private readonly DiscordClient _discordClient;

        public BasePermissionController(
            DiscordClient discordClient)
        {
            _discordClient = discordClient;
        }

        protected async Task<IActionResult?> CheckUserPermissionsAsync(ulong guildUlongId)
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
}