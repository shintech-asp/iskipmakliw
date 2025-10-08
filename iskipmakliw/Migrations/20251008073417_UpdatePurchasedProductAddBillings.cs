using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iskipmakliw.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePurchasedProductAddBillings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BillingsId",
                table: "PurchasedProduct",
                type: "int",
                nullable: false,
                defaultValue: 2);

            migrationBuilder.CreateIndex(
                name: "IX_PurchasedProduct_BillingsId",
                table: "PurchasedProduct",
                column: "BillingsId");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchasedProduct_Billings_BillingsId",
                table: "PurchasedProduct",
                column: "BillingsId",
                principalTable: "Billings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchasedProduct_Billings_BillingsId",
                table: "PurchasedProduct");

            migrationBuilder.DropIndex(
                name: "IX_PurchasedProduct_BillingsId",
                table: "PurchasedProduct");

            migrationBuilder.DropColumn(
                name: "BillingsId",
                table: "PurchasedProduct");
        }
    }
}
