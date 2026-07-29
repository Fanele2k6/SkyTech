using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkyTech.Migrations
{
    /// <inheritdoc />
    public partial class LinkJobRoleKnowledgeToPosition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PositionId",
                table: "JobRoleKnowledges",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_JobRoleKnowledges_PositionId",
                table: "JobRoleKnowledges",
                column: "PositionId");

            migrationBuilder.AddForeignKey(
                name: "FK_JobRoleKnowledges_Positions_PositionId",
                table: "JobRoleKnowledges",
                column: "PositionId",
                principalTable: "Positions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobRoleKnowledges_Positions_PositionId",
                table: "JobRoleKnowledges");

            migrationBuilder.DropIndex(
                name: "IX_JobRoleKnowledges_PositionId",
                table: "JobRoleKnowledges");

            migrationBuilder.DropColumn(
                name: "PositionId",
                table: "JobRoleKnowledges");
        }
    }
}
