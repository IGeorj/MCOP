﻿using DSharpPlus.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MCOP.Data.Models
{
    public class BotStatus
    {
        public const int StatusLimit = 64;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required, MaxLength(StatusLimit)]
        public string Status { get; set; } = null!;

        [Required]
        public ActivityType Activity { get; set; } = ActivityType.Playing;
    }
}
