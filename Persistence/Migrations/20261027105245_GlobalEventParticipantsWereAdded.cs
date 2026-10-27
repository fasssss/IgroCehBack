using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class GlobalEventParticipantsWereAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GlobalEventParticipants",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GlobalEventId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RolledAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    PickedGameId = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PickedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GlobalEventParticipants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GlobalEventParticipants_GlobalEventGames_PickedGameId",
                        column: x => x.PickedGameId,
                        principalTable: "GlobalEventGames",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GlobalEventParticipants_GlobalEvents_GlobalEventId",
                        column: x => x.GlobalEventId,
                        principalTable: "GlobalEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GlobalEventParticipants_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "GlobalEventRolledGames",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ParticipantId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GlobalEventGameId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Order = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GlobalEventRolledGames", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GlobalEventRolledGames_GlobalEventGames_GlobalEventGameId",
                        column: x => x.GlobalEventGameId,
                        principalTable: "GlobalEventGames",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GlobalEventRolledGames_GlobalEventParticipants_ParticipantId",
                        column: x => x.ParticipantId,
                        principalTable: "GlobalEventParticipants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_GlobalEventParticipants_GlobalEventId_UserId",
                table: "GlobalEventParticipants",
                columns: new[] { "GlobalEventId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GlobalEventParticipants_PickedGameId",
                table: "GlobalEventParticipants",
                column: "PickedGameId");

            migrationBuilder.CreateIndex(
                name: "IX_GlobalEventParticipants_UserId",
                table: "GlobalEventParticipants",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_GlobalEventRolledGames_GlobalEventGameId",
                table: "GlobalEventRolledGames",
                column: "GlobalEventGameId");

            migrationBuilder.CreateIndex(
                name: "IX_GlobalEventRolledGames_ParticipantId",
                table: "GlobalEventRolledGames",
                column: "ParticipantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GlobalEventRolledGames");

            migrationBuilder.DropTable(
                name: "GlobalEventParticipants");
        }
    }
}
