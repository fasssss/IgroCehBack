using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ReviewScoreReplacedRecommendation : Migration
    {
        /// <inheritdoc />
        // the thumb gives way to the score of the redesign; the production has no reviews yet,
        // a review of a test database gets the middle score instead of an impossible 0
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRecommended",
                table: "Reviews");

            migrationBuilder.AddColumn<decimal>(
                name: "Score",
                table: "Reviews",
                type: "decimal(3,1)",
                precision: 3,
                scale: 1,
                nullable: false,
                defaultValue: 5m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Score",
                table: "Reviews");

            migrationBuilder.AddColumn<bool>(
                name: "IsRecommended",
                table: "Reviews",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }
    }
}
