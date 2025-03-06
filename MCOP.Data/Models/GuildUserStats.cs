using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MCOP.Data.Models
{
    [PrimaryKey(nameof(GuildId), nameof(UserId))]
    public class GuildUserStats
    {
        public ulong GuildId { get; set; }
        public ulong UserId { get; set; }


        [ConcurrencyCheck]
        public int DuelWin { get; set; } = 0;

        [ConcurrencyCheck]
        public int DuelLose { get; set; } = 0;

        [ConcurrencyCheck]
        public int Likes { get; set; } = 0;

        [ConcurrencyCheck]
        public int Exp { get; set; } = 0;

        public DateTime LastExpAwardedAt { get; set; } = DateTime.MinValue;
    }
}
