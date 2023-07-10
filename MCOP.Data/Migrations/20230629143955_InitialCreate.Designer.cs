﻿// <auto-generated />
using System;
using MCOP.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace MCOP.Data.Migrations
{
    [DbContext(typeof(McopDbContext))]
    [Migration("20230629143955_InitialCreate")]
    partial class InitialCreate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "7.0.5");

            modelBuilder.Entity("MCOP.Data.Models.BotStatus", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("Activity")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("BotStatuses");
                });

            modelBuilder.Entity("MCOP.Data.Models.Guild", b =>
                {
                    b.Property<ulong>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("Guilds");
                });

            modelBuilder.Entity("MCOP.Data.Models.GuildConfig", b =>
                {
                    b.Property<ulong>("GuildId")
                        .HasColumnType("INTEGER");

                    b.Property<ulong?>("LewdChannelId")
                        .HasColumnType("INTEGER");

                    b.Property<ulong?>("LogChannelId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Prefix")
                        .IsRequired()
                        .HasMaxLength(8)
                        .HasColumnType("TEXT");

                    b.HasKey("GuildId");

                    b.ToTable("GuildConfigs");
                });

            modelBuilder.Entity("MCOP.Data.Models.GuildMessage", b =>
                {
                    b.Property<ulong>("GuildId")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("Id")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Likes")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("UserId")
                        .HasColumnType("INTEGER");

                    b.HasKey("GuildId", "Id");

                    b.HasIndex("UserId");

                    b.ToTable("GuildMessages");
                });

            modelBuilder.Entity("MCOP.Data.Models.GuildUser", b =>
                {
                    b.Property<ulong>("GuildId")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("UserId")
                        .HasColumnType("INTEGER");

                    b.HasKey("GuildId", "UserId");

                    b.HasIndex("UserId");

                    b.ToTable("GuildUsers");
                });

            modelBuilder.Entity("MCOP.Data.Models.GuildUserStat", b =>
                {
                    b.Property<ulong>("GuildId")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("UserId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("DuelLose")
                        .HasColumnType("INTEGER");

                    b.Property<int>("DuelWin")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Likes")
                        .HasColumnType("INTEGER");

                    b.HasKey("GuildId", "UserId");

                    b.ToTable("GuildUserStats");
                });

            modelBuilder.Entity("MCOP.Data.Models.ImageHash", b =>
                {
                    b.Property<ulong>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("GuildId")
                        .HasColumnType("INTEGER");

                    b.Property<byte[]>("Hash")
                        .IsRequired()
                        .HasColumnType("BLOB");

                    b.Property<ulong>("MessageId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("GuildId", "MessageId");

                    b.ToTable("ImageHashes");
                });

            modelBuilder.Entity("MCOP.Data.Models.User", b =>
                {
                    b.Property<ulong>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("MCOP.Data.Models.GuildConfig", b =>
                {
                    b.HasOne("MCOP.Data.Models.Guild", "Guild")
                        .WithOne("GuildConfig")
                        .HasForeignKey("MCOP.Data.Models.GuildConfig", "GuildId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Guild");
                });

            modelBuilder.Entity("MCOP.Data.Models.GuildMessage", b =>
                {
                    b.HasOne("MCOP.Data.Models.Guild", "Guild")
                        .WithMany()
                        .HasForeignKey("GuildId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MCOP.Data.Models.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Guild");

                    b.Navigation("User");
                });

            modelBuilder.Entity("MCOP.Data.Models.GuildUser", b =>
                {
                    b.HasOne("MCOP.Data.Models.Guild", "Guild")
                        .WithMany("GuildUsers")
                        .HasForeignKey("GuildId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MCOP.Data.Models.User", "User")
                        .WithMany("GuildUsers")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Guild");

                    b.Navigation("User");
                });

            modelBuilder.Entity("MCOP.Data.Models.GuildUserStat", b =>
                {
                    b.HasOne("MCOP.Data.Models.GuildUser", "GuildUser")
                        .WithMany()
                        .HasForeignKey("GuildId", "UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("GuildUser");
                });

            modelBuilder.Entity("MCOP.Data.Models.ImageHash", b =>
                {
                    b.HasOne("MCOP.Data.Models.GuildMessage", "GuildMessage")
                        .WithMany("ImageHashes")
                        .HasForeignKey("GuildId", "MessageId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("GuildMessage");
                });

            modelBuilder.Entity("MCOP.Data.Models.Guild", b =>
                {
                    b.Navigation("GuildConfig")
                        .IsRequired();

                    b.Navigation("GuildUsers");
                });

            modelBuilder.Entity("MCOP.Data.Models.GuildMessage", b =>
                {
                    b.Navigation("ImageHashes");
                });

            modelBuilder.Entity("MCOP.Data.Models.User", b =>
                {
                    b.Navigation("GuildUsers");
                });
#pragma warning restore 612, 618
        }
    }
}