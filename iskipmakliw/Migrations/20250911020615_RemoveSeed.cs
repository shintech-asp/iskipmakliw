using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iskipmakliw.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "DateArchived", "DateCreated", "DateModified", "Email", "Password", "Username" },
                values: new object[] { 1, null, new DateTime(2025, 9, 11, 10, 1, 52, 71, DateTimeKind.Local).AddTicks(5650), null, "admin@test.com", "AQAAAAIAAYagAAAAEBht0McoUBftognT/s0s8vhOV5nZS7YloQWmVKjCXDKQk92hujDMtJuZ+lK5rf3ksQ==", "Administrator" });
        }
    }
}
