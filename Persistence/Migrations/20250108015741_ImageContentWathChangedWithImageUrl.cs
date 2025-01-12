using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ImageContentWathChangedWithImageUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageContent",
                table: "Games");

            migrationBuilder.RenameColumn(
                name: "ImageType",
                table: "Games",
                newName: "ImageUrl");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "Games",
                newName: "ImageType");

            migrationBuilder.AddColumn<byte[]>(
                name: "ImageContent",
                table: "Games",
                type: "longblob",
                nullable: true);
        }
    }
}
