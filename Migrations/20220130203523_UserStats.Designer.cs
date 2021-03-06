// <auto-generated />
using MCOP.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace MCOP.Migrations
{
    [DbContext(typeof(BotDbContext))]
    [Migration("20220130203523_UserStats")]
    partial class UserStats
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("xf")
                .HasAnnotation("ProductVersion", "6.0.0");

            modelBuilder.Entity("MCOP.Database.Models.BotStatus", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("id");

                    b.Property<int>("Activity")
                        .HasColumnType("INTEGER")
                        .HasColumnName("activity_type");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("TEXT")
                        .HasColumnName("status");

                    b.HasKey("Id");

                    b.ToTable("bot_statuses", "xf");
                });

            modelBuilder.Entity("MCOP.Database.Models.PrivilegedUser", b =>
                {
                    b.Property<long>("UserIdDb")
                        .HasColumnType("INTEGER")
                        .HasColumnName("uid");

                    b.HasKey("UserIdDb");

                    b.ToTable("privileged_users", "xf");
                });

            modelBuilder.Entity("MCOP.Database.Models.UserStats", b =>
                {
                    b.Property<long>("GuildIdDb")
                        .HasColumnType("INTEGER")
                        .HasColumnName("gid");

                    b.Property<long>("UserIdDb")
                        .HasColumnType("INTEGER")
                        .HasColumnName("uid");

                    b.Property<int>("DuelLose")
                        .HasColumnType("INTEGER")
                        .HasColumnName("duel_lose");

                    b.Property<int>("DuelWin")
                        .HasColumnType("INTEGER")
                        .HasColumnName("duel_win");

                    b.Property<int>("Like")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasDefaultValue(0)
                        .HasColumnName("like");

                    b.HasKey("GuildIdDb", "UserIdDb");

                    b.ToTable("user_stats", "xf");
                });
#pragma warning restore 612, 618
        }
    }
}
