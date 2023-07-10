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
    public class ActivityService : IScoped
    {
        private readonly McopDbContext _context;
        public UptimeInformation UptimeInformation { get; }

        public ActivityService(McopDbContext context)
        {
            _context = context;
            UptimeInformation = new UptimeInformation(Process.GetCurrentProcess().StartTime);
        }

        public async Task<BotStatus> GetRandomStatusAsync()
        {
            try
            {
                List<BotStatus> statuses = await _context.BotStatuses.ToListAsync();

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
