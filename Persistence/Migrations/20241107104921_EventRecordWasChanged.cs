using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EventRecordWasChanged : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventRecords_Games_GameId",
                table: "EventRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_EventRecords_Users_FromUserId",
                table: "EventRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_EventRecords_Users_ToUserId",
                table: "EventRecords");

            migrationBuilder.RenameColumn(
                name: "FromUserId",
                table: "EventRecords",
                newName: "ParticipantId");

            migrationBuilder.RenameIndex(
                name: "IX_EventRecords_FromUserId",
                table: "EventRecords",
                newName: "IX_EventRecords_ParticipantId");

            migrationBuilder.AlterColumn<string>(
                name: "ToUserId",
                table: "EventRecords",
                type: "varchar(255)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255)")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "GameId",
                table: "EventRecords",
                type: "varchar(255)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255)")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddForeignKey(
                name: "FK_EventRecords_Games_GameId",
                table: "EventRecords",
                column: "GameId",
                principalTable: "Games",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EventRecords_Users_ParticipantId",
                table: "EventRecords",
                column: "ParticipantId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EventRecords_Users_ToUserId",
                table: "EventRecords",
                column: "ToUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventRecords_Games_GameId",
                table: "EventRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_EventRecords_Users_ParticipantId",
                table: "EventRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_EventRecords_Users_ToUserId",
                table: "EventRecords");

            migrationBuilder.RenameColumn(
                name: "ParticipantId",
                table: "EventRecords",
                newName: "FromUserId");

            migrationBuilder.RenameIndex(
                name: "IX_EventRecords_ParticipantId",
                table: "EventRecords",
                newName: "IX_EventRecords_FromUserId");

            migrationBuilder.UpdateData(
                table: "EventRecords",
                keyColumn: "ToUserId",
                keyValue: null,
                column: "ToUserId",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "ToUserId",
                table: "EventRecords",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "EventRecords",
                keyColumn: "GameId",
                keyValue: null,
                column: "GameId",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "GameId",
                table: "EventRecords",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddForeignKey(
                name: "FK_EventRecords_Games_GameId",
                table: "EventRecords",
                column: "GameId",
                principalTable: "Games",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EventRecords_Users_FromUserId",
                table: "EventRecords",
                column: "FromUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EventRecords_Users_ToUserId",
                table: "EventRecords",
                column: "ToUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
