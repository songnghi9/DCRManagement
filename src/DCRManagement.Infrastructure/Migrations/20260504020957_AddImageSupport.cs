using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DCRManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddImageSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Attachments_DCRId",
                table: "Attachments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Attachments_DCRId",
                table: "Attachments",
                column: "DCRId");
        }
    }
}
