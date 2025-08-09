namespace MCOP.Core.Models
{
    public sealed record ImageHashDto(
        ulong Id,
        ulong MessageId,
        ulong GuildId,
        byte[] Hash
    );
}
