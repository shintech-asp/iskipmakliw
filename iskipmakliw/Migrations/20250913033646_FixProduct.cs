using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iskipmakliw.Migrations
{
    /// <inheritdoc />
    public partial class FixProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Gallery_Products_ProductId",
                table: "Gallery");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductsVariants_Products_ProductId",
                table: "ProductsVariants");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Products",
                table: "Products");

            migrationBuilder.RenameTable(
                name: "Products",
                newName: "Product");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Product",
                table: "Product",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Gallery_Product_ProductId",
                table: "Gallery",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductsVariants_Product_ProductId",
                table: "ProductsVariants",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Gallery_Product_ProductId",
                table: "Gallery");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductsVariants_Product_ProductId",
                table: "ProductsVariants");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Product",
                table: "Product");

            migrationBuilder.RenameTable(
                name: "Product",
                newName: "Products");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Products",
                table: "Products",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Gallery_Products_ProductId",
                table: "Gallery",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductsVariants_Products_ProductId",
                table: "ProductsVariants",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
