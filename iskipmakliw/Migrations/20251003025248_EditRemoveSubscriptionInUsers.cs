using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iskipmakliw.Migrations
{
    /// <inheritdoc />
    public partial class EditRemoveSubscriptionInUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Subscription",
                table: "UserDetails");

            migrationBuilder.DropColumn(
                name: "SubscriptionDuration",
                table: "UserDetails");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Subscription",
                table: "UserDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SubscriptionDuration",
                table: "UserDetails",
                type: "int",
                nullable: true);
        }
    }
}
