using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MCOP.Data.Models
{
    public class ImageHash
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public ulong Id { get; set; }

        public ulong MessageId { get; set; }

        public ulong GuildId { get; set; }

        public required byte[] Hash { get; set; }


        [ForeignKey($"{nameof(GuildId)},{nameof(MessageId)}")]
        public GuildMessage GuildMessage { get; set; } = null!;
    }
}
