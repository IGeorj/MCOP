using System.ComponentModel.DataAnnotations;

namespace MCOP.Data.Models
{
    public class Guild
    {
        [Key]
        public ulong Id { get; set; }

        public ICollection<GuildUser> GuildUsers { get; set; }

        public GuildConfig GuildConfig { get; set; } = null!;
    }
}
