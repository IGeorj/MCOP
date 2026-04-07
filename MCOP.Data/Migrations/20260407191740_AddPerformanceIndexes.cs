using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MCOP.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Index to optimize the reaction count subquery in GetGuildUserStatsAsync
            // Covers: WHERE r.GuildId = ? AND r.Emoji = ? AND r.EmojiId = ?
            // Includes MessageId for the JOIN to GuildMessages
            migrationBuilder.CreateIndex(
                name: "IX_GuildMessageReactions_GuildId_Emoji_EmojiId_MessageId",
                table: "GuildMessageReactions",
                columns: new[] { "GuildId", "Emoji", "EmojiId", "MessageId" });

            // Index to optimize the JOIN between GuildMessageReactions and GuildMessages
            // and the GROUP BY m.UserId in the subquery
            migrationBuilder.CreateIndex(
                name: "IX_GuildMessages_Id_UserId",
                table: "GuildMessages",
                columns: new[] { "Id", "UserId" });

            // Index to optimize the main query WHERE clause and sorting by Exp
            migrationBuilder.CreateIndex(
                name: "IX_GuildUserStats_GuildId_Exp",
                table: "GuildUserStats",
                columns: new[] { "GuildId", "Exp" });

            // Index to optimize sorting by DuelWin
            migrationBuilder.CreateIndex(
                name: "IX_GuildUserStats_GuildId_DuelWin",
                table: "GuildUserStats",
                columns: new[] { "GuildId", "DuelWin" });

            // Index to optimize sorting by DuelLose
            migrationBuilder.CreateIndex(
                name: "IX_GuildUserStats_GuildId_DuelLose",
                table: "GuildUserStats",
                columns: new[] { "GuildId", "DuelLose" });

            // Index to optimize the COUNT query for totalCount
            migrationBuilder.CreateIndex(
                name: "IX_GuildUserStats_GuildId",
                table: "GuildUserStats",
                column: "GuildId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GuildMessageReactions_GuildId_Emoji_EmojiId_MessageId",
                table: "GuildMessageReactions");

            migrationBuilder.DropIndex(
                name: "IX_GuildMessages_Id_UserId",
                table: "GuildMessages");

            migrationBuilder.DropIndex(
                name: "IX_GuildUserStats_GuildId_Exp",
                table: "GuildUserStats");

            migrationBuilder.DropIndex(
                name: "IX_GuildUserStats_GuildId_DuelWin",
                table: "GuildUserStats");

            migrationBuilder.DropIndex(
                name: "IX_GuildUserStats_GuildId_DuelLose",
                table: "GuildUserStats");

            migrationBuilder.DropIndex(
                name: "IX_GuildUserStats_GuildId",
                table: "GuildUserStats");
        }
    }
}
