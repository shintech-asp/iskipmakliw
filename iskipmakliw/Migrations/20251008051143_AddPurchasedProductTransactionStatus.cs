using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iskipmakliw.Migrations
{
    /// <inheritdoc />
    public partial class AddPurchasedProductTransactionStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Status",
                table: "PurchasedProduct",
                newName: "TransactionStatus");

            migrationBuilder.AddColumn<string>(
                name: "PaymentStatus",
                table: "PurchasedProduct",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                table: "PurchasedProduct");

            migrationBuilder.RenameColumn(
                name: "TransactionStatus",
                table: "PurchasedProduct",
                newName: "Status");
        }
    }
}
