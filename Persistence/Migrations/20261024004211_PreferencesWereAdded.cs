using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PreferencesWereAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PreferenceTags",
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
                    table.PrimaryKey("PK_PreferenceTags", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UserPreferences",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PreferenceTagId = table.Column<int>(type: "int", nullable: false),
                    IsAvoided = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPreferences", x => new { x.UserId, x.PreferenceTagId });
                    table.ForeignKey(
                        name: "FK_UserPreferences_PreferenceTags_PreferenceTagId",
                        column: x => x.PreferenceTagId,
                        principalTable: "PreferenceTags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserPreferences_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "PreferenceTags",
                columns: new[] { "Id", "Name", "NameRu" },
                values: new object[,]
                {
                    { 1, "Action", "Экшен" },
                    { 2, "Adventure", "Приключения" },
                    { 3, "RPG", "RPG" },
                    { 4, "A-RPG", "A-RPG" },
                    { 5, "JRPG", "JRPG" },
                    { 6, "MMORPG", "MMORPG" },
                    { 7, "Shooter", "Шутер" },
                    { 8, "Platformer", "Платформер" },
                    { 9, "Metroidvania", "Метроидвания" },
                    { 10, "Soulslike", "Соулслайк" },
                    { 11, "Roguelike", "Рогалик" },
                    { 12, "Strategy", "Стратегия" },
                    { 13, "RTS", "RTS" },
                    { 14, "Turn-based strategy", "Пошаговая стратегия" },
                    { 15, "Tactics", "Тактика" },
                    { 16, "Simulation", "Симулятор" },
                    { 17, "Racing", "Гонки" },
                    { 18, "Sports", "Спорт" },
                    { 19, "Fighting", "Файтинг" },
                    { 20, "Hack and slash", "Слэшер" },
                    { 21, "Beat 'em up", "Битемап" },
                    { 22, "Stealth", "Стелс" },
                    { 23, "Survival", "Выживание" },
                    { 24, "Horror", "Хоррор" },
                    { 25, "Puzzle", "Головоломка" },
                    { 26, "Point-and-click", "Квест" },
                    { 27, "Visual novel", "Визуальная новелла" },
                    { 28, "Rhythm", "Ритм-игра" },
                    { 29, "Sandbox", "Песочница" },
                    { 30, "City builder", "Градостроитель" },
                    { 31, "Card game", "Карточная игра" },
                    { 32, "MOBA", "MOBA" },
                    { 33, "Battle royale", "Королевская битва" },
                    { 34, "...-1999", "...-1999" },
                    { 35, "2000-2009", "2000-2009" },
                    { 36, "2010-2019", "2010-2019" },
                    { 37, "2020-...", "2020-..." },
                    { 38, "AAA", "AAA" },
                    { 39, "Indie", "Инди" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_PreferenceTags_Name",
                table: "PreferenceTags",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PreferenceTags_NameRu",
                table: "PreferenceTags",
                column: "NameRu",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserPreferences_PreferenceTagId",
                table: "UserPreferences",
                column: "PreferenceTagId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserPreferences");

            migrationBuilder.DropTable(
                name: "PreferenceTags");
        }
    }
}
