using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkyTech.Migrations
{
    /// <inheritdoc />
    public partial class DocModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileData",
                table: "OwnerIdentityBooks");

            migrationBuilder.DropColumn(
                name: "FileData",
                table: "DriversLicences");

            migrationBuilder.DropColumn(
                name: "FileData",
                table: "DriverIdentityBooks");

            migrationBuilder.DropColumn(
                name: "FileData",
                table: "BusRegCertificates");

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "OwnerIdentityBooks",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "DriversLicences",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "DriverIdentityBooks",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "BusRegCertificates",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "OwnerIdentityBooks");

            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "DriversLicences");

            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "DriverIdentityBooks");

            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "BusRegCertificates");

            migrationBuilder.AddColumn<byte[]>(
                name: "FileData",
                table: "OwnerIdentityBooks",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<byte[]>(
                name: "FileData",
                table: "DriversLicences",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<byte[]>(
                name: "FileData",
                table: "DriverIdentityBooks",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<byte[]>(
                name: "FileData",
                table: "BusRegCertificates",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);
        }
    }
}
