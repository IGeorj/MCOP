using MCOP.Core.Common;
using MCOP.Core.Models;
using MCOP.Core.Services.Shared.Common;
using MCOP.Data;
using MCOP.Data.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Diagnostics;
using System.Linq;

namespace MCOP.Core.Services.Scoped
{
    public interface IBotStatusesService
    {
        public Task<BotStatusDto> GetRandomStatusAsync();
        public UptimeInformation GetUptimeInfo();
    }

    public sealed class BotStatusesService : IBotStatusesService
    {
        private readonly IDbContextFactory<McopDbContext> _contextFactory;

        public UptimeInformation UptimeInformation { get; }

        public BotStatusesService(IDbContextFactory<McopDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
            UptimeInformation = new UptimeInformation(Process.GetCurrentProcess().StartTime);
        }

        public async Task<BotStatusDto> GetRandomStatusAsync()
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();

                List<BotStatusDto> statuses = await context.BotStatuses.Select(s => new BotStatusDto(s.Id, s.Status, s.Activity)).ToListAsync();

                if (statuses.Count < 1)
                {
                    return new BotStatusDto(0, "Взлом жопы", DSharpPlus.Entities.DiscordActivityType.Playing);
                }

                return new SafeRandom().ChooseRandomElement(statuses);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in GetRandomStatusAsync");
                throw;
            }
        }

        public UptimeInformation GetUptimeInfo()
        {
            try
            {
                return UptimeInformation;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in GetUptimeInfo");
                throw;
            }
        }

    }
}
