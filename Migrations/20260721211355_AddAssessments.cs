using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecruitmentSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddAssessments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AssessmentId",
                table: "QuizQuestions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Assessments",
                columns: table => new
                {
                    AssessmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assessments", x => x.AssessmentId);
                    table.ForeignKey(
                        name: "FK_Assessments_Jobs_JobId",
                        column: x => x.JobId,
                        principalTable: "Jobs",
                        principalColumn: "JobId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuizQuestions_AssessmentId",
                table: "QuizQuestions",
                column: "AssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Assessments_JobId",
                table: "Assessments",
                column: "JobId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_QuizQuestions_Assessments_AssessmentId",
                table: "QuizQuestions",
                column: "AssessmentId",
                principalTable: "Assessments",
                principalColumn: "AssessmentId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuizQuestions_Assessments_AssessmentId",
                table: "QuizQuestions");

            migrationBuilder.DropTable(
                name: "Assessments");

            migrationBuilder.DropIndex(
                name: "IX_QuizQuestions_AssessmentId",
                table: "QuizQuestions");

            migrationBuilder.DropColumn(
                name: "AssessmentId",
                table: "QuizQuestions");
        }
    }
}
