using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;


namespace MCOP.Database
{
    public class GenericRepository<TEntity> where TEntity : class
    {
        protected readonly BotDbContextBuilder builder;

        public GenericRepository(BotDbContextBuilder dbb)
        {
            builder = dbb;
        }

        public async virtual Task<IEnumerable<TEntity>> GetAsync(
            Expression<Func<TEntity, bool>>? filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            string includeProperties = "")
        {
            IQueryable<TEntity> query;
            using (BotDbContext db = builder.CreateContext())
            {
                query = db.Set<TEntity>();
            }

            if (filter != null)
            {
                query = query.Where(filter);
            }

            foreach (var includeProperty in includeProperties.Split
                (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            if (orderBy != null)
            {
                return await orderBy(query).ToListAsync();
            }
            else
            {
                return await query.ToListAsync();
            }
        }

        public async virtual Task<TEntity?> GetByIdAsync(params object[] keys)
        {
            using BotDbContext db = builder.CreateContext();
            return await db.Set<TEntity>().FindAsync(keys);
        }

        public async virtual void AddAsync(TEntity entity)
        {
            using BotDbContext db = builder.CreateContext();
            TEntity? dbEntity = await db.Set<TEntity>().FindAsync(entity);
            if (dbEntity is null)
            {
                await db.Set<TEntity>().AddAsync(entity);
                await db.SaveChangesAsync();
            }
        }

        public async virtual Task<bool> DeleteAsync(params object[] keys)
        {
            using BotDbContext db = builder.CreateContext();
            var dbSet = db.Set<TEntity>();
            TEntity? entityToDelete = await dbSet.FindAsync(keys);
            if (entityToDelete is not null)
            {
                dbSet.Remove(entityToDelete);
                await db.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async virtual void UpdateAsync(TEntity entityToUpdate)
        {
            using BotDbContext db = builder.CreateContext();
            db.Set<TEntity>().Update(entityToUpdate);
            await db.SaveChangesAsync();
        }
    }
}
