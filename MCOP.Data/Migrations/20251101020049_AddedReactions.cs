using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MCOP.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedReactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GuildUserEmojies");

            migrationBuilder.DropColumn(
                name: "Likes",
                table: "GuildMessages");

            migrationBuilder.AddColumn<ulong>(
                name: "LikeEmojiId",
                table: "GuildConfigs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0ul);

            migrationBuilder.AddColumn<string>(
                name: "LikeEmojiName",
                table: "GuildConfigs",
                type: "TEXT",
                nullable: false,
                defaultValue: "❤️");

            migrationBuilder.AddColumn<bool>(
                name: "ReactionTrackingEnabled",
                table: "GuildConfigs",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "GuildMessageReactions",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    MessageId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    EmojiId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    Emoji = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedByUserId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    HistoricalIndex = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildMessageReactions", x => new { x.GuildId, x.MessageId, x.CreatedByUserId, x.HistoricalIndex, x.EmojiId, x.Emoji });
                    table.ForeignKey(
                        name: "FK_GuildMessageReactions_GuildMessages_GuildId_MessageId",
                        columns: x => new { x.GuildId, x.MessageId },
                        principalTable: "GuildMessages",
                        principalColumns: new[] { "GuildId", "Id" },
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GuildMessageReactions");

            migrationBuilder.DropColumn(
                name: "LikeEmojiId",
                table: "GuildConfigs");

            migrationBuilder.DropColumn(
                name: "LikeEmojiName",
                table: "GuildConfigs");

            migrationBuilder.DropColumn(
                name: "ReactionTrackingEnabled",
                table: "GuildConfigs");

            migrationBuilder.AddColumn<int>(
                name: "Likes",
                table: "GuildMessages",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "GuildUserEmojies",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    UserId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    EmojiId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    RecievedAmount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildUserEmojies", x => new { x.GuildId, x.UserId, x.EmojiId });
                });
        }
    }
}
