using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCOP.Database.Models
{
    [Table("user_messages")]
    public class UserMessage : IEquatable<UserMessage>
    {
        [Column("gid")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long GuildIdDb { get; set; }
        [NotMapped]
        public ulong GuildId { get => (ulong)this.GuildIdDb; set => this.GuildIdDb = (long)value; }

        [Column("mid")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long MessageIdDb { get; set; }
        [NotMapped]
        public ulong MessageId { get => (ulong)this.MessageIdDb; set => this.MessageIdDb = (long)value; }


        public virtual ICollection<ImageHash> Hashes { get; set; }



        public bool Equals(UserMessage? other)
        {
            return other is { } && this.GuildId == other.GuildId && this.MessageId == other.MessageId;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as UserMessage);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.GuildId, this.MessageId);
        }
    }
}
