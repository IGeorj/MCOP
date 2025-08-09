namespace MCOP.Core.Models
{
    public sealed record GuildUserEmojiDto(
        ulong GuildId,
        ulong UserId,
        ulong EmojiId,
        int RecievedAmount
    );
}
