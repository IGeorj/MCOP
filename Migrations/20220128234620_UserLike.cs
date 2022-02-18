using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MCOP.Migrations
{
    public partial class UserLike : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "xf");

            migrationBuilder.CreateTable(
                name: "bot_statuses",
                schema: "xf",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    status = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    activity_type = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bot_statuses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "privileged_users",
                schema: "xf",
                columns: table => new
                {
                    uid = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_privileged_users", x => x.uid);
                });

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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bot_statuses",
                schema: "xf");

            migrationBuilder.DropTable(
                name: "privileged_users",
                schema: "xf");

            migrationBuilder.DropTable(
                name: "user_like",
                schema: "xf");
        }
    }
}
