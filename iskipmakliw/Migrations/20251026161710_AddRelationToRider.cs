using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iskipmakliw.Migrations
{
    /// <inheritdoc />
    public partial class AddRelationToRider : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_DeliverProduct_RiderId",
                table: "DeliverProduct",
                column: "RiderId");

            migrationBuilder.AddForeignKey(
                name: "FK_DeliverProduct_Users_RiderId",
                table: "DeliverProduct",
                column: "RiderId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeliverProduct_Users_RiderId",
                table: "DeliverProduct");

            migrationBuilder.DropIndex(
                name: "IX_DeliverProduct_RiderId",
                table: "DeliverProduct");
        }
    }
}
