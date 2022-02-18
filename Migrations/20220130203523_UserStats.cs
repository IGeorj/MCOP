using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MCOP.Migrations
{
    public partial class UserStats : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_like",
                schema: "xf");

            migrationBuilder.CreateTable(
                name: "user_stats",
                schema: "xf",
                columns: table => new
                {
                    gid = table.Column<long>(type: "INTEGER", nullable: false),
                    uid = table.Column<long>(type: "INTEGER", nullable: false),
                    like = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    duel_win = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    duel_lose = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_stats", x => new { x.gid, x.uid });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_stats",
                schema: "xf");

            migrationBuilder.CreateTable(
                name: "user_like",
                schema: "xf",
                columns: table => new
                {
                    gid = table.Column<long>(type: "INTEGER", nullable: false),
                    uid = table.Column<long>(type: "INTEGER", nullable: false),
                    likes = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_like", x => new { x.gid, x.uid });
                });
        }
    }
}
