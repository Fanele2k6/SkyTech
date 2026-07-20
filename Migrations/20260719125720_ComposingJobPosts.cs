using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkyTech.Migrations
{
    /// <inheritdoc />
    public partial class ComposingJobPosts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "JobPosts",
                columns: table => new
                {
                    PostId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobTitle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Department = table.Column<int>(type: "int", nullable: false),
                    Location = table.Column<int>(type: "int", nullable: false),
                    EmploymentType = table.Column<int>(type: "int", nullable: false),
                    SalaryMin = table.Column<double>(type: "float", nullable: true),
                    SalaryMax = table.Column<double>(type: "float", nullable: true),
                    Responsibilities = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: false),
                    RequiredCerts = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequiredSkillsAndXP = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClosingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PostStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobPosts", x => x.PostId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobPosts");
        }
    }
}
