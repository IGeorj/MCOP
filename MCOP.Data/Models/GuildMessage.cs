using Microsoft.EntityFrameworkCore;

namespace MCOP.Data.Models
{
    [PrimaryKey(nameof(GuildId), nameof(Id))]
    public sealed class GuildMessage
    {
        public ulong GuildId { get; set; }

        public ulong Id { get; set; }

        public ulong UserId { get; set; }

        public ulong ChannelId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<ImageHash> ImageHashes { get; set; } = [];

        public ICollection<GuildMessageReaction> Reactions { get; set; } = [];
    }
}