using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iskipmakliw.Migrations
{
    /// <inheritdoc />
    public partial class CustomizationTableFk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_CustomizationOrders_SellersId",
                table: "CustomizationOrders",
                column: "SellersId");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomizationOrders_Users_SellersId",
                table: "CustomizationOrders",
                column: "SellersId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomizationOrders_Users_SellersId",
                table: "CustomizationOrders");

            migrationBuilder.DropIndex(
                name: "IX_CustomizationOrders_SellersId",
                table: "CustomizationOrders");
        }
    }
}
