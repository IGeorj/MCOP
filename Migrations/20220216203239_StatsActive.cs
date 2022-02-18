using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MCOP.Migrations
{
    public partial class StatsActive : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "active",
                schema: "xf",
                table: "user_stats",
                type: "INTEGER",
                nullable: false,
                defaultValue: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "active",
                schema: "xf",
                table: "user_stats");
        }
    }
}
