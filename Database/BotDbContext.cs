using MCOP.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace MCOP.Database;

public class BotDbContext : DbContext
{
    #region db sets
    public virtual DbSet<BotStatus> BotStatuses { get; protected set; }
    public virtual DbSet<PrivilegedUser> PrivilegedUsers { get; protected set; }
    public virtual DbSet<UserStats> UserStats { get; protected set; }
    public virtual DbSet<UserMessage> UserMessages { get; protected set; }
    public virtual DbSet<ImageHash> ImageHashes { get; protected set; }
    #endregion

    private BotDbProvider Provider { get; }
    private string ConnectionString { get; }


#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    public BotDbContext(BotDbProvider provider, string connectionString)
    {
        this.Provider = provider;
        this.ConnectionString = connectionString;
    }

    public BotDbContext(BotDbProvider provider, string connectionString, DbContextOptions<BotDbContext> options)
        : base(options)
    {
        this.Provider = provider;
        this.ConnectionString = connectionString;
    }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder.IsConfigured)
            return;

        switch (this.Provider) {
            case BotDbProvider.PostgreSql:
                optionsBuilder.UseNpgsql(this.ConnectionString);
                break;
            case BotDbProvider.Sqlite:
            case BotDbProvider.SqliteInMemory:
                optionsBuilder.UseSqlite(this.ConnectionString);
                break;
            default:
                throw new NotSupportedException("Selected database provider not supported!");
        }
    }

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.HasDefaultSchema("xf");

        mb.Entity<UserStats>().HasKey(us => new { us.GuildIdDb, us.UserIdDb });
        mb.Entity<UserStats>().Property(us => us.Like).HasDefaultValue(0);
        mb.Entity<UserStats>().Property(us => us.DuelWin).HasDefaultValue(0);
        mb.Entity<UserStats>().Property(us => us.DuelLose).HasDefaultValue(0);
        mb.Entity<UserStats>().Property(us => us.IsActive).HasDefaultValue(true);
        mb.Entity<UserMessage>().HasKey(um => new { um.GuildIdDb, um.MessageIdDb });
        mb.Entity<ImageHash>().HasKey(ih => ih.Id);
        mb.Entity<ImageHash>().HasOne(um => um.Message).WithMany(ih => ih.Hashes).HasForeignKey(um => new { um.GuildIdDb, um.MessageIdDb });
    }
}
