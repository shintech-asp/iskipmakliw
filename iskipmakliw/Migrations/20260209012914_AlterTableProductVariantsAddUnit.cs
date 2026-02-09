using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iskipmakliw.Migrations
{
    /// <inheritdoc />
    public partial class AlterTableProductVariantsAddUnit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Dimension",
                table: "ProductVariants",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "Unit",
                table: "ProductVariants",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Unit",
                table: "ProductVariants");

            migrationBuilder.AlterColumn<string>(
                name: "Dimension",
                table: "ProductVariants",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
