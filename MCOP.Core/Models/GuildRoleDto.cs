namespace MCOP.Core.Models
{
    public sealed record GuildRoleDto(
        ulong GuildId,
        ulong Id,
        int? LevelToGetRole,
        bool IsGainExpBlocked,
        string? LevelUpMessageTemplate
    );
}
