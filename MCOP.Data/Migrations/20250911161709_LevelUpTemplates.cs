using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MCOP.Data.Migrations
{
    /// <inheritdoc />
    public partial class LevelUpTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LevelUpMessageTemplate",
                table: "GuildRoles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Prefix",
                table: "GuildConfigs",
                type: "TEXT",
                maxLength: 8,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 8,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LevelUpMessageTemplate",
                table: "GuildConfigs",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "LevelUpMessagesEnabled",
                table: "GuildConfigs",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LevelUpMessageTemplate",
                table: "GuildRoles");

            migrationBuilder.DropColumn(
                name: "LevelUpMessageTemplate",
                table: "GuildConfigs");

            migrationBuilder.DropColumn(
                name: "LevelUpMessagesEnabled",
                table: "GuildConfigs");

            migrationBuilder.AlterColumn<string>(
                name: "Prefix",
                table: "GuildConfigs",
                type: "TEXT",
                maxLength: 8,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 8);
        }
    }
}
