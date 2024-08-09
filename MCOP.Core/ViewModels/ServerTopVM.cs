using MCOP.Data.Models;

namespace MCOP.Core.ViewModels
{
    public class ServerTopVM
    {
        public List<GuildUserStat> TopLikedUser { get; set; } = new List<GuildUserStat>();
        public List<GuildUserStat> TopDuelUser { get; set; } = new List<GuildUserStat>();
        public List<GuildUserStat> HonorableMention { get; set; } = new List<GuildUserStat>();
    }
}
