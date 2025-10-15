using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iskipmakliw.Migrations
{
    /// <inheritdoc />
    public partial class FixJoining : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Gallery_Product_ProductId",
                table: "Gallery");

            migrationBuilder.AddColumn<int>(
                name: "UsersId",
                table: "Product",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "pUsersId",
                table: "Product",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "ProductId",
                table: "Gallery",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Product_UsersId",
                table: "Product",
                column: "UsersId");

            migrationBuilder.AddForeignKey(
                name: "FK_Gallery_Product_ProductId",
                table: "Gallery",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Product_Users_UsersId",
                table: "Product",
                column: "UsersId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Gallery_Product_ProductId",
                table: "Gallery");

            migrationBuilder.DropForeignKey(
                name: "FK_Product_Users_UsersId",
                table: "Product");

            migrationBuilder.DropIndex(
                name: "IX_Product_UsersId",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "UsersId",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "pUsersId",
                table: "Product");

            migrationBuilder.AlterColumn<int>(
                name: "ProductId",
                table: "Gallery",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Gallery_Product_ProductId",
                table: "Gallery",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "Id");
        }
    }
}
