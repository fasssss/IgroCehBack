using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CatalogCoverPathsWereFixed : Migration
    {
        /// <inheritdoc />
        // a cover the catalog doesn't take from Steam came as a path on its site (/api/img?u=...&w=480) and was saved as it was,
        // so the front looked for it on our API; the covers uploaded by players are paths on our API too, they stay
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "UPDATE Games SET ImageUrl = CONCAT('https://videogamescritic.com', ImageUrl) " +
                "WHERE VgcId IS NOT NULL AND ImageUrl LIKE '/%' AND ImageUrl NOT LIKE '//%' AND ImageUrl NOT LIKE '/Upload/%';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // the broken paths aren't brought back
        }
    }
}
