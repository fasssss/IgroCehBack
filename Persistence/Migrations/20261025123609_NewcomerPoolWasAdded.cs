using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class NewcomerPoolWasAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AllowsNewcomers",
                table: "Events",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "NewcomerPoolGameId",
                table: "EventRecords",
                type: "varchar(255)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "NewcomerPoolGames",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EventId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GameId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AuthorId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewcomerPoolGames", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NewcomerPoolGames_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NewcomerPoolGames_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NewcomerPoolGames_Users_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_EventRecords_NewcomerPoolGameId",
                table: "EventRecords",
                column: "NewcomerPoolGameId");

            migrationBuilder.CreateIndex(
                name: "IX_NewcomerPoolGames_AuthorId",
                table: "NewcomerPoolGames",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_NewcomerPoolGames_EventId_GameId",
                table: "NewcomerPoolGames",
                columns: new[] { "EventId", "GameId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NewcomerPoolGames_GameId",
                table: "NewcomerPoolGames",
                column: "GameId");

            migrationBuilder.AddForeignKey(
                name: "FK_EventRecords_NewcomerPoolGames_NewcomerPoolGameId",
                table: "EventRecords",
                column: "NewcomerPoolGameId",
                principalTable: "NewcomerPoolGames",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventRecords_NewcomerPoolGames_NewcomerPoolGameId",
                table: "EventRecords");

            migrationBuilder.DropTable(
                name: "NewcomerPoolGames");

            migrationBuilder.DropIndex(
                name: "IX_EventRecords_NewcomerPoolGameId",
                table: "EventRecords");

            migrationBuilder.DropColumn(
                name: "AllowsNewcomers",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "NewcomerPoolGameId",
                table: "EventRecords");
        }
    }
}
