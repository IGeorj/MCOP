using DSharpPlus.Entities;

namespace MCOP.Common.Helpers
{
    public static class PermissionsHelper
    {
        public static bool HasManageServerPermission(string permissionBits)
        {
            if (!ulong.TryParse(permissionBits, out var bits))
                return false;

            var permissions = (DiscordPermission)bits;

            return permissions.HasFlag(DiscordPermission.Administrator) ||
                   permissions.HasFlag(DiscordPermission.ManageGuild);
        }
    }
}
