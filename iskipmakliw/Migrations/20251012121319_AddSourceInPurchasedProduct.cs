using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iskipmakliw.Migrations
{
    /// <inheritdoc />
    public partial class AddSourceInPurchasedProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchasedProduct_ProductVariants_ProductVariantsId",
                table: "PurchasedProduct");

            migrationBuilder.AlterColumn<int>(
                name: "ProductVariantsId",
                table: "PurchasedProduct",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "CustomizationOrdersId",
                table: "PurchasedProduct",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "PurchasedProduct",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_PurchasedProduct_CustomizationOrdersId",
                table: "PurchasedProduct",
                column: "CustomizationOrdersId");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchasedProduct_CustomizationOrders_CustomizationOrdersId",
                table: "PurchasedProduct",
                column: "CustomizationOrdersId",
                principalTable: "CustomizationOrders",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchasedProduct_ProductVariants_ProductVariantsId",
                table: "PurchasedProduct",
                column: "ProductVariantsId",
                principalTable: "ProductVariants",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchasedProduct_CustomizationOrders_CustomizationOrdersId",
                table: "PurchasedProduct");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchasedProduct_ProductVariants_ProductVariantsId",
                table: "PurchasedProduct");

            migrationBuilder.DropIndex(
                name: "IX_PurchasedProduct_CustomizationOrdersId",
                table: "PurchasedProduct");

            migrationBuilder.DropColumn(
                name: "CustomizationOrdersId",
                table: "PurchasedProduct");

            migrationBuilder.DropColumn(
                name: "Source",
                table: "PurchasedProduct");

            migrationBuilder.AlterColumn<int>(
                name: "ProductVariantsId",
                table: "PurchasedProduct",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchasedProduct_ProductVariants_ProductVariantsId",
                table: "PurchasedProduct",
                column: "ProductVariantsId",
                principalTable: "ProductVariants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
