﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

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

#nullable restore