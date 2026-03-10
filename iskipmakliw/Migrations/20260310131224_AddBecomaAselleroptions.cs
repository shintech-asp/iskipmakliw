using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iskipmakliw.Migrations
{
    /// <inheritdoc />
    public partial class AddBecomaAselleroptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CardNumber",
                table: "UserDetails",
                newName: "PaymentNumber");

            migrationBuilder.AddColumn<string>(
                name: "AccountName",
                table: "UserDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AccountNumber",
                table: "UserDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Ewallet",
                table: "UserDetails",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountName",
                table: "UserDetails");

            migrationBuilder.DropColumn(
                name: "AccountNumber",
                table: "UserDetails");

            migrationBuilder.DropColumn(
                name: "Ewallet",
                table: "UserDetails");

            migrationBuilder.RenameColumn(
                name: "PaymentNumber",
                table: "UserDetails",
                newName: "CardNumber");
        }
    }
}
