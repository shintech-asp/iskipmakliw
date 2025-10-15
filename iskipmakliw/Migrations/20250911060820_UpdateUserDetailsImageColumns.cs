using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iskipmakliw.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserDetailsImageColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop old column
            migrationBuilder.DropColumn(
                name: "GovernmentId",
                table: "UserDetails");

            // Recreate with correct type
            migrationBuilder.AddColumn<byte[]>(
                name: "GovernmentId",
                table: "UserDetails",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AlterColumn<byte[]>(
                name: "CapturedId",
                table: "UserDetails",
                type: "varbinary(max)",
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "varbinary(max)");
        }
        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "GovernmentId",
                table: "UserDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(byte[]),
                oldType: "varbinary(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<byte[]>(
                name: "CapturedId",
                table: "UserDetails",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0],
                oldClrType: typeof(byte[]),
                oldType: "varbinary(max)",
                oldNullable: true);
        }
    }
}
