using MCOP.Data.Models;

namespace MCOP.Core.ViewModels
{
    public class ServerTopVM
    {
        public List<GuildUserStats> TopLikedUser { get; set; } = new List<GuildUserStats>();
        public List<GuildUserStats> TopDuelUser { get; set; } = new List<GuildUserStats>();
        public List<GuildUserStats> HonorableMention { get; set; } = new List<GuildUserStats>();
    }
}
