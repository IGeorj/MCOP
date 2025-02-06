using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace MCOP.Data.Models
{
    [PrimaryKey(nameof(GuildId), nameof(UserId), nameof(EmojiId))]
    public class GuildUserEmoji
    {
        public ulong GuildId { get; set; }
        public ulong UserId { get; set; }
        public int RecievedAmount { get; set; } = 0;

        [ForeignKey($"{nameof(GuildId)},{nameof(UserId)}")]
        public GuildUser GuildUser { get; set; }

        [ForeignKey(nameof(GuildEmoji))]
        public ulong EmojiId { get; set; }

        public GuildEmoji GuildEmoji { get; set; }
    }
}
