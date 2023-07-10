using DSharpPlus;
using DSharpPlus.Entities;

namespace MCOP.Extensions;

internal static class DiscordClientExtensions
{
    public static bool IsOwnedBy(this DiscordClient client, DiscordUser user)
        => client.CurrentApplication?.Owners.Contains(user) ?? false;
}
