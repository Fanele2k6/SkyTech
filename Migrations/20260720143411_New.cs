using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkyTech.Migrations
{
    /// <inheritdoc />
    public partial class New : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "PostStatus",
                table: "JobPosts",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "OwnerId",
                table: "JobPosts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_JobPosts_OwnerId",
                table: "JobPosts",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_JobPosts_shopowners_OwnerId",
                table: "JobPosts",
                column: "OwnerId",
                principalTable: "shopowners",
                principalColumn: "OwnerId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobPosts_shopowners_OwnerId",
                table: "JobPosts");

            migrationBuilder.DropIndex(
                name: "IX_JobPosts_OwnerId",
                table: "JobPosts");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "JobPosts");

            migrationBuilder.AlterColumn<string>(
                name: "PostStatus",
                table: "JobPosts",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
