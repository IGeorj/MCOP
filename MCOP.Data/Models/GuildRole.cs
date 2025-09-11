using Microsoft.EntityFrameworkCore;

namespace MCOP.Data.Models
{
    [PrimaryKey(nameof(GuildId), nameof(Id))]
    public sealed class GuildRole
    {
        public ulong GuildId { get; set; }

        public ulong Id { get; set; }

        public int? LevelToGetRole { get; set; } = null;

        public string? LevelUpMessageTemplate { get; set; }

        public bool IsGainExpBlocked { get; set; } = false;
    }
}