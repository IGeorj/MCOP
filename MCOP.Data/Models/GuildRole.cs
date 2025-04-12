using Microsoft.EntityFrameworkCore;

#nullable disable

namespace MCOP.Data.Models
{
    [PrimaryKey(nameof(GuildId), nameof(Id))]
    public class GuildRole
    {
        public ulong GuildId { get; set; }

        public ulong Id { get; set; }

        public int? LevelToGetRole { get; set; } = null;
    }
}

#nullable restore