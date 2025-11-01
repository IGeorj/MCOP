using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace MCOP.Data.Models
{
    [PrimaryKey(nameof(GuildId), nameof(MessageId), nameof(CreatedByUserId), nameof(HistoricalIndex), nameof(EmojiId), nameof(Emoji))]
    public sealed class GuildMessageReaction
    {
        public ulong GuildId { get; set; } 
        public ulong MessageId { get; set; }
        public ulong EmojiId { get; set; } = 0;
        public string Emoji { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ulong CreatedByUserId { get; set; }  // 0 for historical/unknown users

        public int HistoricalIndex { get; set; } = 0; // 0 for new reactions, >0 for historical/unknown users

        [ForeignKey($"{nameof(GuildId)},{nameof(MessageId)}")]
        public GuildMessage? Message { get; set; }
    }
}
