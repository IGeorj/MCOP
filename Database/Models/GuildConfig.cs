using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MCOP.Database.Models
{

    [Table("guild_config")]
    public class GuildConfig
    {
        [Key]
        [Column("gid")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long GuildIdDb { get; set; }
        [NotMapped]
        public ulong GuildId { get => (ulong)GuildIdDb; set => GuildIdDb = (long)value; }

        [Column("prefix")]
        [MaxLength(8)]
        public string? Prefix { get; set; }

        [Column("log_cid")]
        public long LogChannelIdDb { get; set; }
        [NotMapped]
        public ulong LogChannelId { get => (ulong)LogChannelIdDb; set => LogChannelIdDb = (long)value; }
        [NotMapped]
        public bool LoggingEnabled => LogChannelId != default;

        [Column("lewd_cid")]
        public long LewdChannelIdDb { get; set; }
        [NotMapped]
        public ulong LewdChannelId { get => (ulong)LewdChannelIdDb; set => LewdChannelIdDb = (long)value; }

        [NotMapped]
        public bool LewdEnabled => LewdChannelId != default;

    }
}
