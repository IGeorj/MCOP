using MCOP.Database;
using MCOP.Database.Models;
using MCOP.Services;
using Microsoft.EntityFrameworkCore;

namespace MCOP.Modules.Basic.Services
{
    public sealed class UserStatsService : DbAbstractionServiceBase<UserStats, ulong, ulong>
    {

        public UserStatsService(BotDbContextBuilder dbb) : base(dbb) 
        {

        }

        public override DbSet<UserStats> DbSetSelector(BotDbContext db)
            => db.UserStats;
        public override IQueryable<UserStats> GroupSelector(IQueryable<UserStats> bds, ulong gid)
            => bds.Where(bd => bd.GuildIdDb == (long)gid);
        public override ulong EntityGroupSelector(UserStats bd)
            => bd.GuildId;
        public override UserStats EntityFactory(ulong gid, ulong uid) => new UserStats { GuildId = gid, UserId = uid };
        public override ulong EntityIdSelector(UserStats entity) => entity.UserId;
        public override object[] EntityPrimaryKeySelector(ulong gid, ulong uid) => new object[] { (long)gid, (long)uid };


        public async Task<IReadOnlyList<UserStats>> GetTopLikedUsersAsync(ulong? gid = null, int count = 5)
        {
            await using BotDbContext db = dbb.CreateContext();
            return gid is null
                ? await DbSetSelector(db).AsQueryable().Where(u => u.IsActive == true).OrderByDescending(u => u.Like).Take(count).ToListAsync()
                : await GroupSelector(DbSetSelector(db), gid.Value).Where(u => u.IsActive == true).OrderByDescending(u => u.Like).Take(count).ToListAsync();
        }

        public async Task<IReadOnlyList<UserStats>> GetTopDuelUsersAsync(ulong? gid = null, int count = 5)
        {
            await using BotDbContext db = dbb.CreateContext();
            return gid is null
                ? await DbSetSelector(db).AsQueryable().Where(u => u.IsActive == true).OrderByDescending(u => u.DuelWin).Take(count).ToListAsync()
                : await GroupSelector(DbSetSelector(db), gid.Value).Where(u => u.IsActive == true).OrderByDescending(u => u.DuelWin).Take(count).ToListAsync();
        }

        public async Task<UserStats?> GetKekDuelUserAsync(ulong? gid = null)
        {
            await using BotDbContext db = dbb.CreateContext();
            return gid is null
                ? DbSetSelector(db).AsQueryable().Where(u => u.IsActive == true && u.DuelWin >= 5 && u.DuelLose >= 5).OrderByDescending(u => u.DuelLose - u.DuelWin).FirstOrDefault()
                : GroupSelector(DbSetSelector(db), gid.Value).Where(u => u.IsActive == true && u.DuelWin > 5 && u.DuelLose > 5).OrderByDescending(u => u.DuelLose - u.DuelWin).FirstOrDefault();
        }

        public async Task ChangeLikeAsync(ulong gid, ulong uid, int change = 1)
        {
            UserStats stats = await GetOrAddAsync(gid, uid);
            stats.Like += change;

            await using BotDbContext db = this.dbb.CreateContext();
            db.Update(stats);
            await db.SaveChangesAsync();
        }

        public async Task AddLoseAsync(ulong gid, ulong uid, int change = 1)
        {
            UserStats stats = await GetOrAddAsync(gid, uid);
            stats.DuelLose += change;

            await using BotDbContext db = this.dbb.CreateContext();
            db.Update(stats);
            await db.SaveChangesAsync();
        }

        public async Task AddWinAsync(ulong gid, ulong uid, int change = 1)
        {
            UserStats stats = await GetOrAddAsync(gid, uid);
            stats.DuelWin += change;

            await using BotDbContext db = this.dbb.CreateContext();
            db.Update(stats);
            await db.SaveChangesAsync();
        }


        public async Task<UserStats> GetOrAddAsync(ulong gid, ulong uid)
        {
            UserStats? stats = await this.GetAsync(gid, uid);

            if (stats is not null)
            {
                return stats;
            }

            await this.AddAsync(EntityFactory(gid, uid));
            return await this.GetAsync(gid, uid);
        }

        public async Task ChangeActiveStatus(ulong gid, ulong uid, bool active)
        {
            UserStats stats = await GetOrAddAsync(gid, uid);
            stats.IsActive = active;

            await using BotDbContext db = this.dbb.CreateContext();
            db.Update(stats);
            await db.SaveChangesAsync();
        }


    }
}
