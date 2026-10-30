using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PassingChecksWereMoved : Migration
    {
        /// <inheritdoc />
        // a column per platform became a row per platform, so the participant doesn't change for a new platform;
        // the checks move before the columns go
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GlobalEventPassingChecks",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ParticipantId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Platform = table.Column<int>(type: "int", nullable: false),
                    CheckedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GlobalEventPassingChecks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GlobalEventPassingChecks_GlobalEventParticipants_Participant~",
                        column: x => x.ParticipantId,
                        principalTable: "GlobalEventParticipants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_GlobalEventPassingChecks_ParticipantId_Platform",
                table: "GlobalEventPassingChecks",
                columns: new[] { "ParticipantId", "Platform" },
                unique: true);

            migrationBuilder.Sql(
                "INSERT INTO GlobalEventPassingChecks (Id, ParticipantId, Platform, CheckedAt, CreatedAt) " +
                "SELECT UUID(), Id, 1, SteamCheckedAt, SteamCheckedAt FROM GlobalEventParticipants WHERE SteamCheckedAt IS NOT NULL;");
            migrationBuilder.Sql(
                "INSERT INTO GlobalEventPassingChecks (Id, ParticipantId, Platform, CheckedAt, CreatedAt) " +
                "SELECT UUID(), Id, 2, XboxCheckedAt, XboxCheckedAt FROM GlobalEventParticipants WHERE XboxCheckedAt IS NOT NULL;");

            // the passings made before Xbox were made by Steam (1), so the platform is empty only for a claim
            migrationBuilder.Sql(
                "UPDATE GlobalEventParticipants p SET p.PassedPlatform = 1 " +
                "WHERE p.PassedAt IS NOT NULL AND p.PassedPlatform IS NULL AND NOT EXISTS (" +
                "SELECT 1 FROM GlobalEventClaims c WHERE c.ParticipantId = p.Id AND c.GlobalEventGameId = p.PickedGameId AND c.Status = 1);");

            migrationBuilder.DropColumn(
                name: "SteamCheckedAt",
                table: "GlobalEventParticipants");

            migrationBuilder.DropColumn(
                name: "XboxCheckedAt",
                table: "GlobalEventParticipants");
        }

        /// <inheritdoc />
        // the platform of the old Steam passings stays, it is true either way
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SteamCheckedAt",
                table: "GlobalEventParticipants",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "XboxCheckedAt",
                table: "GlobalEventParticipants",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.Sql(
                "UPDATE GlobalEventParticipants p JOIN GlobalEventPassingChecks pc ON pc.ParticipantId = p.Id AND pc.Platform = 1 " +
                "SET p.SteamCheckedAt = pc.CheckedAt;");
            migrationBuilder.Sql(
                "UPDATE GlobalEventParticipants p JOIN GlobalEventPassingChecks pc ON pc.ParticipantId = p.Id AND pc.Platform = 2 " +
                "SET p.XboxCheckedAt = pc.CheckedAt;");

            migrationBuilder.DropTable(
                name: "GlobalEventPassingChecks");
        }
    }
}
