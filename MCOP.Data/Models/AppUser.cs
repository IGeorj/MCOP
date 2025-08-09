using System.ComponentModel.DataAnnotations;

namespace MCOP.Data.Models
{
    public sealed class AppUser
    {
        [Key]
        public string Id { get; set; } = null!;
        public string DiscordAccessToken { get; set; } = null!;
        public string DiscordRefreshToken { get; set; } = null!;
        public DateTime DiscordTokenExpiresAt { get; set; }
    }

}
