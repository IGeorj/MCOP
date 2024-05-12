using DSharpPlus.Entities;

namespace MCOP.Extensions;

internal static class DiscordChannelExtensions
{
    public static bool IsNsfwOrNsfwName(this DiscordChannel channel)
        => channel.IsNSFW || channel.Name.StartsWith("nsfw", StringComparison.InvariantCultureIgnoreCase);
}
