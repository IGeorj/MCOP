using MCOP.Data.Models;
using MCOP.Utils;
using Microsoft.EntityFrameworkCore;

namespace MCOP.Data
{
    public class McopDbContext : DbContext
    {
        public virtual DbSet<GuildUserStats> GuildUserStats { get; protected set; }
        public virtual DbSet<GuildConfig> GuildConfigs { get; protected set; }
        public virtual DbSet<GuildMessage> GuildMessages { get; protected set; }
        public virtual DbSet<ImageHash> ImageHashes { get; protected set; }
        public virtual DbSet<BotStatus> BotStatuses { get; protected set; }
        public virtual DbSet<GuildUserEmoji> GuildUserEmojies { get; protected set; }
        public virtual DbSet<GuildRole> GuildRoles { get; protected set; }
        public virtual DbSet<ApiUsage> ApiUsages { get; protected set; }
        public virtual DbSet<AppUser> AppUsers { get; protected set; }

        public McopDbContext() { }
        public McopDbContext(DbContextOptions<McopDbContext> options) : base(options) { }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            ConfigurationService configurationService = new ConfigurationService();
            AsyncExecutionService asyncExecution = new AsyncExecutionService();
            BotConfiguration config = asyncExecution.Execute(configurationService.LoadConfigAsync());
            optionsBuilder.UseSqlite($"Data Source={config.DatabaseConfig.DatabaseName}.db;");
        }
    }
}
