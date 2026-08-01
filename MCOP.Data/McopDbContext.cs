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
        public virtual DbSet<ImageVerificationChannel> ImageVerificationChannels { get; protected set; }
        public virtual DbSet<BotStatus> BotStatuses { get; protected set; }
        public virtual DbSet<GuildRole> GuildRoles { get; protected set; }
        public virtual DbSet<AppUser> AppUsers { get; protected set; }
        public virtual DbSet<GuildMessageReaction> GuildMessageReactions { get; protected set; }

        public McopDbContext() { }
        public McopDbContext(DbContextOptions<McopDbContext> options) : base(options) { }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            ConfigurationService configurationService = new();
            AsyncExecutionService asyncExecution = new();
            BotConfiguration config = asyncExecution.Execute(configurationService.LoadConfigAsync());
            optionsBuilder.UseNpgsql($"Host=localhost;Port=5432;Database={config.DatabaseConfig.DatabaseName};Username={config.DatabaseConfig.Username};Password={config.DatabaseConfig.Password}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GuildUserStatsProjection>(eb =>
            {
                eb.HasNoKey();
                eb.ToView(null); // Not tied to a real table/view
            });
        }
    }
}
