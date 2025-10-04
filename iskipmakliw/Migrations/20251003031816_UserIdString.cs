using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iskipmakliw.Migrations
{
    /// <inheritdoc />
    public partial class UserIdString : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop old binary columns
            migrationBuilder.DropColumn(
                name: "GovernmentId",
                table: "UserDetails");

            migrationBuilder.DropColumn(
                name: "CapturedId",
                table: "UserDetails");

            // Add new string columns for file paths
            migrationBuilder.AddColumn<string>(
                name: "GovernmentIdPath",
                table: "UserDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CapturedIdPath",
                table: "UserDetails",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove string columns
            migrationBuilder.DropColumn(
                name: "GovernmentIdPath",
                table: "UserDetails");

            migrationBuilder.DropColumn(
                name: "CapturedIdPath",
                table: "UserDetails");

            // Re-add the old binary columns in case of rollback
            migrationBuilder.AddColumn<byte[]>(
                name: "GovernmentId",
                table: "UserDetails",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "CapturedId",
                table: "UserDetails",
                type: "varbinary(max)",
                nullable: true);
        }
    }
}
