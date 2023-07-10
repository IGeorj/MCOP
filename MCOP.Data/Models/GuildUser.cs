using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MCOP.Data.Models
{
    [PrimaryKey(nameof(GuildId), nameof(UserId))]
    public class GuildUser
    {
        [ForeignKey(nameof(Guild))]
        public ulong GuildId { get; set; }

        [ForeignKey(nameof(User))]
        public ulong UserId { get; set; }
        public Guild Guild { get; set; }
        public User User { get; set; }
    }
}
