using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class StatusIdOrderWasAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "EventStatuses",
                type: "int",
                nullable: false,
                defaultValue: 0);

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
                column: "Order",
                value: 2);

            migrationBuilder.UpdateData(
                table: "EventStatuses",
                keyColumn: "Id",
                keyValue: 2,
                column: "Order",
                value: 1);

            migrationBuilder.UpdateData(
                table: "EventStatuses",
                keyColumn: "Id",
                keyValue: 3,
                column: "Order",
                value: 4);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Order",
                table: "EventStatuses");
        }
    }
}
