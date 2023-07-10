using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MCOP.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BotStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Activity = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BotStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Guilds",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Guilds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GuildConfigs",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    Prefix = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    LogChannelId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    LewdChannelId = table.Column<ulong>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildConfigs", x => x.GuildId);
                    table.ForeignKey(
                        name: "FK_GuildConfigs_Guilds_GuildId",
                        column: x => x.GuildId,
                        principalTable: "Guilds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GuildMessages",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    Id = table.Column<ulong>(type: "INTEGER", nullable: false),
                    UserId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    Likes = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildMessages", x => new { x.GuildId, x.Id });
                    table.ForeignKey(
                        name: "FK_GuildMessages_Guilds_GuildId",
                        column: x => x.GuildId,
                        principalTable: "Guilds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GuildMessages_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GuildUsers",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    UserId = table.Column<ulong>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildUsers", x => new { x.GuildId, x.UserId });
                    table.ForeignKey(
                        name: "FK_GuildUsers_Guilds_GuildId",
                        column: x => x.GuildId,
                        principalTable: "Guilds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GuildUsers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ImageHashes",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MessageId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    GuildId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    Hash = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImageHashes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImageHashes_GuildMessages_GuildId_MessageId",
                        columns: x => new { x.GuildId, x.MessageId },
                        principalTable: "GuildMessages",
                        principalColumns: new[] { "GuildId", "Id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GuildUserStats",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    UserId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    DuelWin = table.Column<int>(type: "INTEGER", nullable: false),
                    DuelLose = table.Column<int>(type: "INTEGER", nullable: false),
                    Likes = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildUserStats", x => new { x.GuildId, x.UserId });
                    table.ForeignKey(
                        name: "FK_GuildUserStats_GuildUsers_GuildId_UserId",
                        columns: x => new { x.GuildId, x.UserId },
                        principalTable: "GuildUsers",
                        principalColumns: new[] { "GuildId", "UserId" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GuildMessages_UserId",
                table: "GuildMessages",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_GuildUsers_UserId",
                table: "GuildUsers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ImageHashes_GuildId_MessageId",
                table: "ImageHashes",
                columns: new[] { "GuildId", "MessageId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BotStatuses");

            migrationBuilder.DropTable(
                name: "GuildConfigs");

            migrationBuilder.DropTable(
                name: "GuildUserStats");

            migrationBuilder.DropTable(
                name: "ImageHashes");

            migrationBuilder.DropTable(
                name: "GuildUsers");

            migrationBuilder.DropTable(
                name: "GuildMessages");

            migrationBuilder.DropTable(
                name: "Guilds");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
