using System.ComponentModel.DataAnnotations;

namespace MCOP.Data.Models
{
    public class User
    {
        [Key]
        public ulong Id { get; set; }

        public ICollection<GuildUser> GuildUsers { get; set; }
    }
}
