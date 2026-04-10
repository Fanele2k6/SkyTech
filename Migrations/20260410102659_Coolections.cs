using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkyTech.Migrations
{
    /// <inheritdoc />
    public partial class Coolections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DriverId",
                table: "OrderItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAssingedForCollection",
                table: "OrderItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsAssingedForSecDelivery",
                table: "OrderItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_DriverId",
                table: "OrderItems",
                column: "DriverId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_DeliveryDrivers_DriverId",
                table: "OrderItems",
                column: "DriverId",
                principalTable: "DeliveryDrivers",
                principalColumn: "DriverId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_DeliveryDrivers_DriverId",
                table: "OrderItems");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_DriverId",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "DriverId",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "IsAssingedForCollection",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "IsAssingedForSecDelivery",
                table: "OrderItems");
        }
    }
}
