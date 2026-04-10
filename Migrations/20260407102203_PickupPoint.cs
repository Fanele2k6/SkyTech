using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkyTech.Migrations
{
    /// <inheritdoc />
    public partial class PickupPoint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OrderType",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PickupOrderId",
                table: "Orders",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "InterBranchTransfers",
                columns: table => new
                {
                    TransferId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderItemId = table.Column<int>(type: "int", nullable: false),
                    SourceBranchId = table.Column<int>(type: "int", nullable: false),
                    TargetBranchId = table.Column<int>(type: "int", nullable: false),
                    DriverId = table.Column<int>(type: "int", nullable: true),
                    TransferCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TransferStatus = table.Column<int>(type: "int", nullable: false),
                    ScheduledTransferDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeliveredToTargetOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDelivered = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterBranchTransfers", x => x.TransferId);
                    table.ForeignKey(
                        name: "FK_InterBranchTransfers_DeliveryDrivers_DriverId",
                        column: x => x.DriverId,
                        principalTable: "DeliveryDrivers",
                        principalColumn: "DriverId");
                    table.ForeignKey(
                        name: "FK_InterBranchTransfers_OrderItems_OrderItemId",
                        column: x => x.OrderItemId,
                        principalTable: "OrderItems",
                        principalColumn: "OrderItemId");
                    table.ForeignKey(
                        name: "FK_InterBranchTransfers_shopowners_SourceBranchId",
                        column: x => x.SourceBranchId,
                        principalTable: "shopowners",
                        principalColumn: "OwnerId");
                    table.ForeignKey(
                        name: "FK_InterBranchTransfers_shopowners_TargetBranchId",
                        column: x => x.TargetBranchId,
                        principalTable: "shopowners",
                        principalColumn: "OwnerId");
                });

            migrationBuilder.CreateTable(
                name: "PickupOrders",
                columns: table => new
                {
                    PickupOrderId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    PickupBranchId = table.Column<int>(type: "int", nullable: false),
                    PickupReference = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PickupFee = table.Column<double>(type: "float", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CollectedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ScheduledPickupDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PickupOrders", x => x.PickupOrderId);
                    table.ForeignKey(
                        name: "FK_PickupOrders_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "OrderId");
                    table.ForeignKey(
                        name: "FK_PickupOrders_shopowners_PickupBranchId",
                        column: x => x.PickupBranchId,
                        principalTable: "shopowners",
                        principalColumn: "OwnerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InterBranchTransfers_DriverId",
                table: "InterBranchTransfers",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_InterBranchTransfers_OrderItemId",
                table: "InterBranchTransfers",
                column: "OrderItemId");

            migrationBuilder.CreateIndex(
                name: "IX_InterBranchTransfers_SourceBranchId",
                table: "InterBranchTransfers",
                column: "SourceBranchId");

            migrationBuilder.CreateIndex(
                name: "IX_InterBranchTransfers_TargetBranchId",
                table: "InterBranchTransfers",
                column: "TargetBranchId");

            migrationBuilder.CreateIndex(
                name: "IX_PickupOrders_OrderId",
                table: "PickupOrders",
                column: "OrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PickupOrders_PickupBranchId",
                table: "PickupOrders",
                column: "PickupBranchId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InterBranchTransfers");

            migrationBuilder.DropTable(
                name: "PickupOrders");

            migrationBuilder.DropColumn(
                name: "OrderType",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PickupOrderId",
                table: "Orders");
        }
    }
}
