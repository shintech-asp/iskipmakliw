using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iskipmakliw.Migrations
{
    /// <inheritdoc />
    public partial class OnetoMany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PaymentsId",
                table: "UserDetails",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserDetails_PaymentsId",
                table: "UserDetails",
                column: "PaymentsId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserDetails_Payments_PaymentsId",
                table: "UserDetails",
                column: "PaymentsId",
                principalTable: "Payments",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserDetails_Payments_PaymentsId",
                table: "UserDetails");

            migrationBuilder.DropIndex(
                name: "IX_UserDetails_PaymentsId",
                table: "UserDetails");

            migrationBuilder.DropColumn(
                name: "PaymentsId",
                table: "UserDetails");
        }
    }
}
