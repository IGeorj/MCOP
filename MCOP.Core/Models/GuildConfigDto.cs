
namespace MCOP.Core.Models
{
    public sealed record GuildConfigDto(
        ulong GuildId,
        string Prefix,
        ulong? LogChannelId,
        bool LoggingEnabled,
        ulong? LewdChannelId,
        bool LewdEnabled,
        string? LevelUpMessageTemplate,
        bool LevelUpMessagesEnabled,
        string? LikeEmojiName,
        ulong LikeEmojiId,
        bool ReactionTrackingEnabled
    );
}
