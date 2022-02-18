using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Serilog;

namespace MCOP.Extensions;

internal static class DiscordGuildExtensions
{
    public static async Task<bool> HasMember(this DiscordGuild guild, ulong uid)
        => await GetMemberSilentAsync(guild, uid) is { };

    public static async Task<DiscordMember?> GetMemberSilentAsync(this DiscordGuild guild, ulong uid)
    {
        try {
            return await guild.GetMemberAsync(uid);
        } catch (NotFoundException) {
            Log.Information($"GetMemberSilentAsync cant find uid:{uid}");
            return null;
        }
    }
}
