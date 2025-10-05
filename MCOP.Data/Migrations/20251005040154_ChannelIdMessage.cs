using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MCOP.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChannelIdMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<ulong>(
                name: "ChannelId",
                table: "GuildMessages",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0ul);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChannelId",
                table: "GuildMessages");
        }
    }
}
