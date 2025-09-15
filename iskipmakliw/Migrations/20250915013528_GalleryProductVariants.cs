using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iskipmakliw.Migrations
{
    /// <inheritdoc />
    public partial class GalleryProductVariants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProductVariantsId",
                table: "Gallery",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Gallery_ProductVariantsId",
                table: "Gallery",
                column: "ProductVariantsId");

            migrationBuilder.AddForeignKey(
                name: "FK_Gallery_ProductVariants_ProductVariantsId",
                table: "Gallery",
                column: "ProductVariantsId",
                principalTable: "ProductVariants",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Gallery_ProductVariants_ProductVariantsId",
                table: "Gallery");

            migrationBuilder.DropIndex(
                name: "IX_Gallery_ProductVariantsId",
                table: "Gallery");

            migrationBuilder.DropColumn(
                name: "ProductVariantsId",
                table: "Gallery");
        }
    }
}
