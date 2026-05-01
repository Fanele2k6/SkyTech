using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkyTech.Migrations
{
    /// <inheritdoc />
    public partial class AddAssignedDriverToProposal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AssignedDriverId",
                table: "ProposeItems",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProposeItems_AssignedDriverId",
                table: "ProposeItems",
                column: "AssignedDriverId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProposeItems_DeliveryDrivers_AssignedDriverId",
                table: "ProposeItems",
                column: "AssignedDriverId",
                principalTable: "DeliveryDrivers",
                principalColumn: "DriverId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProposeItems_DeliveryDrivers_AssignedDriverId",
                table: "ProposeItems");

            migrationBuilder.DropIndex(
                name: "IX_ProposeItems_AssignedDriverId",
                table: "ProposeItems");

            migrationBuilder.DropColumn(
                name: "AssignedDriverId",
                table: "ProposeItems");
        }
    }
}
