using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCOP.Data.Models
{
    public class GuildConfig
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

        [ForeignKey(nameof(GuildId))]
        public Guild Guild { get; set; }
    }
}
