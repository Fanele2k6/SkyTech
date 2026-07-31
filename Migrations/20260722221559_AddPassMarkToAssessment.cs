using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecruitmentSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddPassMarkToAssessment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PassMark",
                table: "Assessments",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PassMark",
                table: "Assessments");
        }
    }
}
