using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class GlobalEventClaimsWereAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PassedAt",
                table: "GlobalEventParticipants",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GlobalEventClaims",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ParticipantId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GlobalEventGameId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Platform = table.Column<int>(type: "int", nullable: false),
                    Profile = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DecidedById = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DecidedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GlobalEventClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GlobalEventClaims_GlobalEventGames_GlobalEventGameId",
                        column: x => x.GlobalEventGameId,
                        principalTable: "GlobalEventGames",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GlobalEventClaims_GlobalEventParticipants_ParticipantId",
                        column: x => x.ParticipantId,
                        principalTable: "GlobalEventParticipants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GlobalEventClaims_Users_DecidedById",
                        column: x => x.DecidedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_GlobalEventClaims_DecidedById",
                table: "GlobalEventClaims",
                column: "DecidedById");

            migrationBuilder.CreateIndex(
                name: "IX_GlobalEventClaims_GlobalEventGameId",
                table: "GlobalEventClaims",
                column: "GlobalEventGameId");

            migrationBuilder.CreateIndex(
                name: "IX_GlobalEventClaims_ParticipantId",
                table: "GlobalEventClaims",
                column: "ParticipantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GlobalEventClaims");

            migrationBuilder.DropColumn(
                name: "PassedAt",
                table: "GlobalEventParticipants");
        }
    }
}
