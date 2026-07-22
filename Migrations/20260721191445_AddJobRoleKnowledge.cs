using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkyTech.Migrations
{
    /// <inheritdoc />
    public partial class AddJobRoleKnowledge : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "JobRoleKnowledges",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobTitle = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CompanyResponsibilities = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequiredSkills = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequiredExperience = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequiredQualifications = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OtherCompanyInformation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobRoleKnowledges", x => x.RoleId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobRoleKnowledges");
        }
    }
}
