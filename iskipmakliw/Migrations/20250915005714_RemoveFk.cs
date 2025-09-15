using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iskipmakliw.Migrations
{
    /// <inheritdoc />
    public partial class RemoveFk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Gallery_Product_ProductId",
                table: "Gallery");

            migrationBuilder.DropForeignKey(
                name: "FK_Gallery_Users_UsersId",
                table: "Gallery");

            migrationBuilder.AlterColumn<int>(
                name: "UsersId",
                table: "Gallery",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "ProductId",
                table: "Gallery",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "pProductId",
                table: "Gallery",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "pUsersId",
                table: "Gallery",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_Gallery_Product_ProductId",
                table: "Gallery",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Gallery_Users_UsersId",
                table: "Gallery",
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
                name: "FK_Gallery_Users_UsersId",
                table: "Gallery");

            migrationBuilder.DropColumn(
                name: "pProductId",
                table: "Gallery");

            migrationBuilder.DropColumn(
                name: "pUsersId",
                table: "Gallery");

            migrationBuilder.AlterColumn<int>(
                name: "UsersId",
                table: "Gallery",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ProductId",
                table: "Gallery",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Gallery_Product_ProductId",
                table: "Gallery",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Gallery_Users_UsersId",
                table: "Gallery",
                column: "UsersId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
