using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace MCOP.Data.Models
{
    [PrimaryKey(nameof(GuildId), nameof(Id))]
    public sealed class GuildMessage
    {
        public ulong GuildId { get; set; }

        public ulong Id { get; set; }

        public ulong UserId { get; set; }

        public int Likes { get; set; } = 0;

        public ICollection<ImageHash> ImageHashes { get; set; } = null!;
    }
}

#nullable restore