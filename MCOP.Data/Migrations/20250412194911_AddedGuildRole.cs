using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MCOP.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedGuildRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GuildRoles",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    Id = table.Column<ulong>(type: "INTEGER", nullable: false),
                    LevelToGetRole = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildRoles", x => new { x.GuildId, x.Id });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GuildRoles");
        }
    }
}
