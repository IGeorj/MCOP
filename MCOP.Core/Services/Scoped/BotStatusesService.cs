using MCOP.Core.Common;
using MCOP.Core.Services.Shared.Common;
using MCOP.Data;
using MCOP.Data.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Diagnostics;

namespace MCOP.Core.Services.Scoped
{
    public interface IBotStatusesService
    {
        public Task<BotStatus> GetRandomStatusAsync();
        public UptimeInformation GetUptimeInfo();
    }

    public class BotStatusesService : IBotStatusesService
    {
        private readonly IDbContextFactory<McopDbContext> _contextFactory;

        public UptimeInformation UptimeInformation { get; }

        public BotStatusesService(IDbContextFactory<McopDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
            UptimeInformation = new UptimeInformation(Process.GetCurrentProcess().StartTime);
        }

        public async Task<BotStatus> GetRandomStatusAsync()
        {
            try
            {
                await using var context = _contextFactory.CreateDbContext();

                List<BotStatus> statuses = await context.BotStatuses.ToListAsync();

                if (statuses.Count < 1)
                {
                    return new BotStatus
                    {
                        Status = "Взлом жопы"
                    };
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
