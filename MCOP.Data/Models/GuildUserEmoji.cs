using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MCOP.Data.Models
{
    [PrimaryKey(nameof(GuildId), nameof(UserId), nameof(EmojiId))]
    public class GuildUserEmoji
    {
        public ulong GuildId { get; set; }
        public ulong UserId { get; set; }
        public ulong EmojiId { get; set; }

        [ConcurrencyCheck]
        public int RecievedAmount { get; set; } = 0;
    }
}
