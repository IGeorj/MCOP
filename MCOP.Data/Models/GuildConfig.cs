using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MCOP.Data.Models
{
    public sealed class GuildConfig
    {
        [Key]
        public ulong GuildId { get; set; }

        [MaxLength(8)]
        public string Prefix { get; set; } = "!m";

        public ulong? LogChannelId { get; set; }

        [NotMapped]
        public bool LoggingEnabled => LogChannelId != default;

        public ulong? LewdChannelId { get; set; }

        [NotMapped]
        public bool LewdEnabled => LewdChannelId != default;

        public string? LevelUpMessageTemplate { get; set; }

        public bool LevelUpMessagesEnabled { get; set; } = true;
    }
}
