using System.ComponentModel;
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


        [NotMapped]
        public bool LoggingEnabled => LogChannelId != default;
        public ulong? LogChannelId { get; set; }


        [NotMapped]
        public bool LewdEnabled => LewdChannelId != default;
        public ulong? LewdChannelId { get; set; }


        public string? LevelUpMessageTemplate { get; set; }
        public bool LevelUpMessagesEnabled { get; set; } = false;

        [DefaultValue("❤️")]
        public string LikeEmojiName { get; set; } = "❤️";
        public ulong LikeEmojiId { get; set; } = 0;    // 0 for Unicode, >0 for custom
        public bool ReactionTrackingEnabled { get; set; } = false;
    }
}
