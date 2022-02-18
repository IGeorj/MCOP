using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCOP.Modules.Nsfw.Services.Boorus
{
    public interface IBooru
    {
        Task<List<DiscordMessage>> SendRandomImagesAsync(DiscordChannel channel, int amount, string tags = "");
        Task<List<DiscordMessage>> SendDailyTopAsync(DiscordChannel channel, int amount, int page = 0);
        Task<bool> isAvailable();
        string GetBaseTags(string tags);
    }
}
