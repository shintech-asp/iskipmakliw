using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iskipmakliw.Migrations
{
    /// <inheritdoc />
    public partial class FixProductVariants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductsVariants_Product_ProductId",
                table: "ProductsVariants");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductsVariants",
                table: "ProductsVariants");

            migrationBuilder.RenameTable(
                name: "ProductsVariants",
                newName: "ProductVariants");

            migrationBuilder.RenameIndex(
                name: "IX_ProductsVariants_ProductId",
                table: "ProductVariants",
                newName: "IX_ProductVariants_ProductId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductVariants",
                table: "ProductVariants",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductVariants_Product_ProductId",
                table: "ProductVariants",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductVariants_Product_ProductId",
                table: "ProductVariants");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductVariants",
                table: "ProductVariants");

            migrationBuilder.RenameTable(
                name: "ProductVariants",
                newName: "ProductsVariants");

            migrationBuilder.RenameIndex(
                name: "IX_ProductVariants_ProductId",
                table: "ProductsVariants",
                newName: "IX_ProductsVariants_ProductId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductsVariants",
                table: "ProductsVariants",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductsVariants_Product_ProductId",
                table: "ProductsVariants",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
