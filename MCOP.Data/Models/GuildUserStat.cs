using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace MCOP.Data.Models
{
    [PrimaryKey(nameof(GuildId), nameof(UserId))]
    public class GuildUserStat
    {
        public ulong GuildId { get; set; }

        public ulong UserId { get; set; }

        public int DuelWin { get; set; } = 0;

        public int DuelLose { get; set; } = 0;

        public int Likes { get; set; } = 0;

        public int Exp { get; set; } = 0;

        public DateTime LastExpAwardedAt { get; set; } = DateTime.MinValue;

        [ForeignKey($"{nameof(GuildId)},{nameof(UserId)}")]
        public GuildUser GuildUser { get; set; }
    }
}
