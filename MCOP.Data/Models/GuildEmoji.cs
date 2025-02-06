using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MCOP.Data.Models
{
    public class GuildEmoji
    {
        [Key]
        public ulong Id { get; set; }

        public required string DiscordName { get; set; }
        
        [ForeignKey(nameof(Guild))]
        public ulong GuildId { get; set; }

        public Guild Guild { get; set; }

    }
}
