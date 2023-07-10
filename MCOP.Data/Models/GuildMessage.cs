using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MCOP.Data.Models
{
    [PrimaryKey(nameof(GuildId), nameof(Id))]
    public class GuildMessage
    {
        [ForeignKey(nameof(Guild))]
        public ulong GuildId { get; set; }

        public ulong Id { get; set; }

        [ForeignKey(nameof(User))]
        public ulong UserId { get; set; }

        public int Likes { get; set; } = 0;

        public Guild Guild { get; set; }
        public User User { get; set; }

        public ICollection<ImageHash> ImageHashes { get; set; }

    }
}
