using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace MCOP.Data.Models
{
    [PrimaryKey(nameof(GuildId), nameof(UserId))]
    public sealed class GuildUserStats
    {
        public ulong GuildId { get; set; }
        public ulong UserId { get; set; }

        public string? Username { get; set; }
        public string? AvatarHash { get; set; }


        public int DuelWin { get; set; } = 0;
        public int DuelLose { get; set; } = 0;

        public int Likes { get; set; } = 0;

        public int Exp { get; set; } = 0;

        public DateTime LastExpAwardedAt { get; set; } = DateTime.MinValue;

        public bool IsWithinExpCooldown(int expCooldownMinutes) => (DateTime.UtcNow - LastExpAwardedAt).TotalMinutes < expCooldownMinutes;
    }
}
