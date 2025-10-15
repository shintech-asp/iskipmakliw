using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iskipmakliw.Migrations
{
    /// <inheritdoc />
    public partial class AddDeliveryPickupAndDropoff : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeliveredOn",
                table: "DeliverProduct",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DropOffOn",
                table: "DeliverProduct",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PickUpOn",
                table: "DeliverProduct",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeliveredOn",
                table: "DeliverProduct");

            migrationBuilder.DropColumn(
                name: "DropOffOn",
                table: "DeliverProduct");

            migrationBuilder.DropColumn(
                name: "PickUpOn",
                table: "DeliverProduct");
        }
    }
}
