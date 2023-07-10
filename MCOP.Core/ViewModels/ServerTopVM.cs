using MCOP.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCOP.Core.ViewModels
{
    public class ServerTopVM
    {
        public List<GuildUserStat> TopLikedUser { get; set; } = new List<GuildUserStat>();
        public List<GuildUserStat> TopDuelUser { get; set; } = new List<GuildUserStat>();
        public GuildUserStat? HonorableMention { get; set; } = new GuildUserStat();
    }
}
