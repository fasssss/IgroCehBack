using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class LinkedAccountsWereMadeNotUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_SteamId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_XboxXuid",
                table: "Users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Users_SteamId",
                table: "Users",
                column: "SteamId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_XboxXuid",
                table: "Users",
                column: "XboxXuid",
                unique: true);
        }
    }
}
