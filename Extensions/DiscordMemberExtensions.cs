using DSharpPlus.Entities;

namespace MCOP.Extensions
{
    public static class DiscordMemberExtensions
    {
        public static bool IsAdmin(this DiscordMember member)
        {
            if (member.IsBot) { return false; }

            return (member.Permissions == DSharpPlus.Permissions.All || member.Permissions == DSharpPlus.Permissions.Administrator);
        }

    }
}
