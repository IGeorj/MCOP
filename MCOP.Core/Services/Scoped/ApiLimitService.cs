using MCOP.Data;
using MCOP.Data.Models;
using MCOP.Utils.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MCOP.Core.Services.Scoped
{
    public class ApiLimitService : IScoped
    {
        private readonly IDbContextFactory<McopDbContext> _contextFactory;
        private readonly SemaphoreSlim _lock = new(1, 1);

        public ApiLimitService(IDbContextFactory<McopDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<int> IncrementUsageAsync()
        {
            await _lock.WaitAsync();
            try
            {
                await using var context = _contextFactory.CreateDbContext();
                var today = DateTime.UtcNow.ToString("yyyy-MM-dd");
                var usage = await context.ApiUsages
                    .FirstOrDefaultAsync(u => u.Date == today);

                if (usage == null)
                {
                    await context.ApiUsages.AddAsync(new ApiUsage { Count = 1 });
                    await context.SaveChangesAsync();
                    return 1;
                }

                if (usage.Count >= 1000)
                    return usage.Count;

                usage.Count++;
                await context.SaveChangesAsync();
                return usage.Count;
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<int> GetTodayUsageAsync()
        {
            await using var context = _contextFactory.CreateDbContext();

            var today = DateTime.UtcNow.ToString("yyyy-MM-dd");
            return await context.ApiUsages
                .Where(u => u.Date == today)
                .Select(u => u.Count)
                .FirstOrDefaultAsync();
        }
    }
}
