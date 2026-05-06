using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkyTech.Migrations
{
    /// <inheritdoc />
    public partial class AddTechnicianFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConfirmPassword",
                table: "Technicians");

            migrationBuilder.AddColumn<int>(
                name: "TechnicianId",
                table: "Receipts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TechnicianId",
                table: "Quotations",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Receipts_TechnicianId",
                table: "Receipts",
                column: "TechnicianId");

            migrationBuilder.CreateIndex(
                name: "IX_Quotations_TechnicianId",
                table: "Quotations",
                column: "TechnicianId");

            migrationBuilder.AddForeignKey(
                name: "FK_Quotations_Technicians_TechnicianId",
                table: "Quotations",
                column: "TechnicianId",
                principalTable: "Technicians",
                principalColumn: "TechnicianId");

            migrationBuilder.AddForeignKey(
                name: "FK_Receipts_Technicians_TechnicianId",
                table: "Receipts",
                column: "TechnicianId",
                principalTable: "Technicians",
                principalColumn: "TechnicianId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Quotations_Technicians_TechnicianId",
                table: "Quotations");

            migrationBuilder.DropForeignKey(
                name: "FK_Receipts_Technicians_TechnicianId",
                table: "Receipts");

            migrationBuilder.DropIndex(
                name: "IX_Receipts_TechnicianId",
                table: "Receipts");

            migrationBuilder.DropIndex(
                name: "IX_Quotations_TechnicianId",
                table: "Quotations");

            migrationBuilder.DropColumn(
                name: "TechnicianId",
                table: "Receipts");

            migrationBuilder.DropColumn(
                name: "TechnicianId",
                table: "Quotations");

            migrationBuilder.AddColumn<string>(
                name: "ConfirmPassword",
                table: "Technicians",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
