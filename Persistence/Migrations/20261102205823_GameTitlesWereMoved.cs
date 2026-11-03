using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class GameTitlesWereMoved : Migration
    {
        /// <inheritdoc />
        // the columns of the game in Xbox became a row per platform, so the game of an event doesn't change for a new platform;
        // the games in Xbox move before the columns go
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GlobalEventGameTitles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GlobalEventGameId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Platform = table.Column<int>(type: "int", nullable: false),
                    TitleRef = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TitleName = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GlobalEventGameTitles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GlobalEventGameTitles_GlobalEventGames_GlobalEventGameId",
                        column: x => x.GlobalEventGameId,
                        principalTable: "GlobalEventGames",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_GlobalEventGameTitles_GlobalEventGameId_Platform",
                table: "GlobalEventGameTitles",
                columns: new[] { "GlobalEventGameId", "Platform" },
                unique: true);

            migrationBuilder.Sql(
                "INSERT INTO GlobalEventGameTitles (Id, GlobalEventGameId, Platform, TitleRef, TitleName, CreatedAt) " +
                "SELECT UUID(), Id, 2, XboxTitleId, COALESCE(XboxTitleName, XboxTitleId), CreatedAt FROM GlobalEventGames WHERE XboxTitleId IS NOT NULL;");

            migrationBuilder.DropColumn(
                name: "XboxTitleId",
                table: "GlobalEventGames");

            migrationBuilder.DropColumn(
                name: "XboxTitleName",
                table: "GlobalEventGames");
        }

        /// <inheritdoc />
        // only the games in Xbox (2) had columns, the games on later platforms are lost with the table
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.Sql(
                "UPDATE GlobalEventGames g JOIN GlobalEventGameTitles t ON t.GlobalEventGameId = g.Id AND t.Platform = 2 " +
                "SET g.XboxTitleId = t.TitleRef, g.XboxTitleName = t.TitleName;");

            migrationBuilder.DropTable(
                name: "GlobalEventGameTitles");
        }
    }
}
