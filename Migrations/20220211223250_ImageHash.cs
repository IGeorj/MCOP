using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MCOP.Migrations
{
    public partial class ImageHash : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "user_messages",
                schema: "xf",
                columns: table => new
                {
                    gid = table.Column<long>(type: "INTEGER", nullable: false),
                    mid = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_messages", x => new { x.gid, x.mid });
                });

            migrationBuilder.CreateTable(
                name: "image_hashes",
                schema: "xf",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    hash = table.Column<byte[]>(type: "BLOB", nullable: false),
                    gid = table.Column<long>(type: "INTEGER", nullable: false),
                    mid = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_image_hashes", x => x.id);
                    table.ForeignKey(
                        name: "FK_image_hashes_user_messages_gid_mid",
                        columns: x => new { x.gid, x.mid },
                        principalSchema: "xf",
                        principalTable: "user_messages",
                        principalColumns: new[] { "gid", "mid" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_image_hashes_gid_mid",
                schema: "xf",
                table: "image_hashes",
                columns: new[] { "gid", "mid" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "image_hashes",
                schema: "xf");

            migrationBuilder.DropTable(
                name: "user_messages",
                schema: "xf");
        }
    }
}
