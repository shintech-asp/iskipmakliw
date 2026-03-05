using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iskipmakliw.Migrations
{
    /// <inheritdoc />
    public partial class getTrackingInHome : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DeliverProduct_PurchasedProductId",
                table: "DeliverProduct");

            migrationBuilder.CreateIndex(
                name: "IX_DeliverProduct_PurchasedProductId",
                table: "DeliverProduct",
                column: "PurchasedProductId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DeliverProduct_PurchasedProductId",
                table: "DeliverProduct");

            migrationBuilder.CreateIndex(
                name: "IX_DeliverProduct_PurchasedProductId",
                table: "DeliverProduct",
                column: "PurchasedProductId");
        }
    }
}
