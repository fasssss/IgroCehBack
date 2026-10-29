using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class XboxAchievementsWereAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "XboxTitleId",
                table: "GlobalEventGames",
                type: "varchar(10)",
                maxLength: 10,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "XboxTitleName",
                table: "GlobalEventGames",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            // every achievement chosen before is a Steam one (GlobalEventAchievementPlatform.Steam)
            migrationBuilder.AddColumn<int>(
                name: "Platform",
                table: "GlobalEventAchievements",
                type: "int",
                nullable: false,
                defaultValue: 1);

            // the new index comes first: MySQL keeps an index on GlobalEventGameId for its foreign key
            migrationBuilder.CreateIndex(
                name: "IX_GlobalEventAchievements_GlobalEventGameId_Platform_ApiName",
                table: "GlobalEventAchievements",
                columns: new[] { "GlobalEventGameId", "Platform", "ApiName" },
                unique: true);

            migrationBuilder.DropIndex(
                name: "IX_GlobalEventAchievements_GlobalEventGameId_ApiName",
                table: "GlobalEventAchievements");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // without the platform an Xbox id could repeat a Steam API name of the same game
            migrationBuilder.Sql("DELETE FROM GlobalEventAchievements WHERE Platform = 2;");

            migrationBuilder.CreateIndex(
                name: "IX_GlobalEventAchievements_GlobalEventGameId_ApiName",
                table: "GlobalEventAchievements",
                columns: new[] { "GlobalEventGameId", "ApiName" },
                unique: true);

            migrationBuilder.DropIndex(
                name: "IX_GlobalEventAchievements_GlobalEventGameId_Platform_ApiName",
                table: "GlobalEventAchievements");

            migrationBuilder.DropColumn(
                name: "XboxTitleId",
                table: "GlobalEventGames");

            migrationBuilder.DropColumn(
                name: "XboxTitleName",
                table: "GlobalEventGames");

            migrationBuilder.DropColumn(
                name: "Platform",
                table: "GlobalEventAchievements");
        }
    }
}
