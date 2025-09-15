using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iskipmakliw.Migrations
{
    /// <inheritdoc />
    public partial class fixGallery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Gallery_Product_ProductId",
                table: "Gallery");

            migrationBuilder.DropColumn(
                name: "pProductId",
                table: "Gallery");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Gallery_Product_ProductId",
                table: "Gallery");

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
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Gallery_Product_ProductId",
                table: "Gallery",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "Id");
        }
    }
}
