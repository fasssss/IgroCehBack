using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class NewEventStatusesWereAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "EventStatuses",
                keyColumn: "Id",
                keyValue: 0,
                column: "Order",
                value: 5);

            migrationBuilder.UpdateData(
                table: "EventStatuses",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Name", "Order", "UserFriendlyName" },
                values: new object[] { "PlayersShuffle", 4, "Players shuffle" });

            migrationBuilder.UpdateData(
                table: "EventStatuses",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Name", "Order", "UserFriendlyName" },
                values: new object[] { "GamesGuessing", 3, "Guessing games" });

            migrationBuilder.UpdateData(
                table: "EventStatuses",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Name", "Order", "UserFriendlyName" },
                values: new object[] { "RevealingGames", 2, "Revealing games" });

            migrationBuilder.InsertData(
                table: "EventStatuses",
                columns: new[] { "Id", "Name", "Order", "UserFriendlyName" },
                values: new object[,]
                {
                    { 4, "Active", 1, "Active" },
                    { 5, "Finished", 6, "Finished" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "EventStatuses",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "EventStatuses",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.UpdateData(
                table: "EventStatuses",
                keyColumn: "Id",
                keyValue: 0,
                column: "Order",
                value: 3);

            migrationBuilder.UpdateData(
                table: "EventStatuses",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Name", "Order", "UserFriendlyName" },
                values: new object[] { "Auction", 2, "Auction" });

            migrationBuilder.UpdateData(
                table: "EventStatuses",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Name", "Order", "UserFriendlyName" },
                values: new object[] { "Active", 1, "Active" });

            migrationBuilder.UpdateData(
                table: "EventStatuses",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Name", "Order", "UserFriendlyName" },
                values: new object[] { "Finished", 4, "Finished" });
        }
    }
}
