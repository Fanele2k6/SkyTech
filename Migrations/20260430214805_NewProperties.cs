using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkyTech.Migrations
{
    /// <inheritdoc />
    public partial class NewProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "SavedOn",
                table: "ProposeItems",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SavedOn",
                table: "ProposeItems");
        }
    }
}
