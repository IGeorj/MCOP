using MCOP.Core.Common;
using MCOP.Core.Exceptions;
using MCOP.Core.Services.Shared.Common;
using MCOP.Data;
using MCOP.Data.Models;
using MCOP.Utils.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace MCOP.Core.Services.Scoped
{
    public class BotStatusesService : IScoped
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
                throw new McopException(ex, ex.Message);
            }

        }
    }
}
