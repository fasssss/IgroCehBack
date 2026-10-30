using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class XboxCheckWasAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PassedPlatform",
                table: "GlobalEventParticipants",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "XboxCheckedAt",
                table: "GlobalEventParticipants",
                type: "datetime(6)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PassedPlatform",
                table: "GlobalEventParticipants");

            migrationBuilder.DropColumn(
                name: "XboxCheckedAt",
                table: "GlobalEventParticipants");
        }
    }
}
