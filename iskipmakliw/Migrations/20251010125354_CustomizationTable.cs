using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iskipmakliw.Migrations
{
    /// <inheritdoc />
    public partial class CustomizationTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CustomizationOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsersId = table.Column<int>(type: "int", nullable: false),
                    SellersId = table.Column<int>(type: "int", nullable: false),
                    Model = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Color = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Texture = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Scale = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Width = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Height = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SellerStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ModeOfPayment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaymentStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TransactionStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomizationOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomizationOrders_Users_UsersId",
                        column: x => x.UsersId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CustomizationChat",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsersId = table.Column<int>(type: "int", nullable: false),
                    SellersId = table.Column<int>(type: "int", nullable: false),
                    CustomizationOrdersId = table.Column<int>(type: "int", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateSent = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateReceived = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsFromBuyer = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomizationChat", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomizationChat_CustomizationOrders_CustomizationOrdersId",
                        column: x => x.CustomizationOrdersId,
                        principalTable: "CustomizationOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CustomizationChat_Users_SellersId",
                        column: x => x.SellersId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CustomizationChat_Users_UsersId",
                        column: x => x.UsersId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomizationChat_CustomizationOrdersId",
                table: "CustomizationChat",
                column: "CustomizationOrdersId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomizationChat_SellersId",
                table: "CustomizationChat",
                column: "SellersId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomizationChat_UsersId",
                table: "CustomizationChat",
                column: "UsersId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomizationOrders_UsersId",
                table: "CustomizationOrders",
                column: "UsersId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomizationChat");

            migrationBuilder.DropTable(
                name: "CustomizationOrders");
        }
    }
}
