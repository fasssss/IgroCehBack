using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class NonSteamLinksWereRemoved : Migration
    {
        /// <inheritdoc />
        // a catalog game outside Steam (its id from 1000000000 on, Application.Consts.CatalogIds) got a Steam link made from its id,
        // which leads nowhere; only such made-up links go
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "UPDATE Games SET SteamUrl = NULL " +
                "WHERE VgcId >= 1000000000 AND SteamUrl = CONCAT('https://store.steampowered.com/app/', VgcId, '/');");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // the made-up links aren't brought back
        }
    }
}
