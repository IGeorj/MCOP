using Microsoft.EntityFrameworkCore;
using MCOP.Data.Models;
using MCOP.Utils;

namespace MCOP.Data
{
    public class McopDbContext : DbContext
    {
        public virtual DbSet<Guild> Guilds { get; protected set; }
        public virtual DbSet<User> Users{ get; protected set; }
        public virtual DbSet<GuildUser> GuildUsers { get; protected set; }
        public virtual DbSet<GuildUserStat> GuildUserStats { get; protected set; }
        public virtual DbSet<GuildConfig> GuildConfigs { get; protected set; }
        public virtual DbSet<GuildMessage> GuildMessages { get; protected set; }
        public virtual DbSet<ImageHash> ImageHashes { get; protected set; }
        public virtual DbSet<BotStatus> BotStatuses { get; protected set; }

        public McopDbContext() { }
        public McopDbContext(DbContextOptions<McopDbContext> options) : base(options) { }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            ConfigurationService configurationService = new ConfigurationService();
            AsyncExecutionService asyncExecution = new AsyncExecutionService();
            BotConfiguration config = asyncExecution.Execute(configurationService.GetCurrentConfigAsync());
            optionsBuilder.UseSqlite($"Data Source={config.DatabaseConfig.DatabaseName}.db;");
            optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        }

    }
}
