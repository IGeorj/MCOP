using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCOP.Database.Models
{
    [Table("user_stats")]
    public class UserStats : IEquatable<UserStats>
    {
        [Column("gid")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long GuildIdDb { get; set; }
        [NotMapped]
        public ulong GuildId { get => (ulong)this.GuildIdDb; set => this.GuildIdDb = (long)value; }

        [Column("uid")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long UserIdDb { get; set; }
        [NotMapped]
        public ulong UserId { get => (ulong)this.UserIdDb; set => this.UserIdDb = (long)value; }

        [Column("like")]
        public int Like { get; set; } = 0;

        [Column("duel_win")]
        public int DuelWin { get; set; } = 0;

        [Column("duel_lose")]
        public int DuelLose { get; set; } = 0;

        [Column("active")]
        public bool IsActive { get; set; } = true;


        public bool Equals(UserStats? other)
        {
            return other is { } && this.GuildId == other.GuildId && this.UserId == other.UserId;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as UserStats);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.GuildId, this.UserId);
        }
    }
}
