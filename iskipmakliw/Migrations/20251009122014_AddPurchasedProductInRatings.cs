using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iskipmakliw.Migrations
{
    /// <inheritdoc />
    public partial class AddPurchasedProductInRatings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PurchasedProductId",
                table: "Ratings",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_Ratings_PurchasedProductId",
                table: "Ratings",
                column: "PurchasedProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ratings_PurchasedProduct_PurchasedProductId",
                table: "Ratings",
                column: "PurchasedProductId",
                principalTable: "PurchasedProduct",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ratings_PurchasedProduct_PurchasedProductId",
                table: "Ratings");

            migrationBuilder.DropIndex(
                name: "IX_Ratings_PurchasedProductId",
                table: "Ratings");

            migrationBuilder.DropColumn(
                name: "PurchasedProductId",
                table: "Ratings");
        }
    }
}
