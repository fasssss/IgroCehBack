using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class GameAspectsWereAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GameAspects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NameRu = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameAspects", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UserGameAspects",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GameAspectId = table.Column<int>(type: "int", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserGameAspects", x => new { x.UserId, x.GameAspectId });
                    table.ForeignKey(
                        name: "FK_UserGameAspects_GameAspects_GameAspectId",
                        column: x => x.GameAspectId,
                        principalTable: "GameAspects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserGameAspects_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "GameAspects",
                columns: new[] { "Id", "Name", "NameRu" },
                values: new object[,]
                {
                    { 1, "Story", "Сюжет" },
                    { 2, "Gameplay", "Геймплей" },
                    { 3, "Visuals", "Визуал" },
                    { 4, "Music", "Музыка" },
                    { 5, "Atmosphere", "Атмосфера" },
                    { 6, "Difficulty", "Сложность" },
                    { 7, "Achievements", "Ачивки" },
                    { 8, "Exploration", "Исследование" },
                    { 9, "Multiplayer", "Мультиплеер" },
                    { 10, "Co-op", "Кооператив" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_GameAspects_Name",
                table: "GameAspects",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GameAspects_NameRu",
                table: "GameAspects",
                column: "NameRu",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserGameAspects_GameAspectId",
                table: "UserGameAspects",
                column: "GameAspectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserGameAspects");

            migrationBuilder.DropTable(
                name: "GameAspects");
        }
    }
}
