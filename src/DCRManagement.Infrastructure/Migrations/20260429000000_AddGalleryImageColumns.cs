using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DCRManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGalleryImageColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add ImageType column: "Before" | "After" | NULL (regular attachment)
            migrationBuilder.AddColumn<string>(
                name: "ImageType",
                table: "Attachments",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            // Add DisplayOrder column: 0-based order within gallery
            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "Attachments",
                type: "int",
                nullable: true);

            // Index to speed up gallery queries: WHERE DCRId = ? AND ImageType = ?
            migrationBuilder.CreateIndex(
                name: "IX_Attachments_DCRId_ImageType",
                table: "Attachments",
                columns: new[] { "DCRId", "ImageType" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Attachments_DCRId_ImageType",
                table: "Attachments");

            migrationBuilder.DropColumn(
                name: "ImageType",
                table: "Attachments");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "Attachments");
        }
    }
}
