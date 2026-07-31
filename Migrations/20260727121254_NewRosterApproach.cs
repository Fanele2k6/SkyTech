using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkyTech.Migrations
{
    /// <inheritdoc />
    public partial class NewRosterApproach : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rosters_Branches_BranchId",
                table: "Rosters");

            migrationBuilder.DropTable(
                name: "HRNotifications");

            migrationBuilder.DropTable(
                name: "ShiftCandidates");

            migrationBuilder.DropTable(
                name: "ShiftTemplates");

            migrationBuilder.DropTable(
                name: "StaffAvailabilitySlots");

            migrationBuilder.DropTable(
                name: "StaffCertifications");

            migrationBuilder.DropTable(
                name: "StaffExperiences");

            migrationBuilder.DropTable(
                name: "StaffLeaves");

            migrationBuilder.DropTable(
                name: "StaffSkills");

            migrationBuilder.DropTable(
                name: "ShiftRequirements");

            migrationBuilder.DropTable(
                name: "Certifications");

            migrationBuilder.DropTable(
                name: "Skills");

            migrationBuilder.RenameColumn(
                name: "ShiftDate",
                table: "RosterShifts",
                newName: "WorkDate");

            migrationBuilder.AddColumn<int>(
                name: "BranchId",
                table: "RosterShifts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PositionId",
                table: "RosterShifts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StaffId",
                table: "RosterShifts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Rosters",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "WeekEndDate",
                table: "Rosters",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "StaffRotationOrders",
                columns: table => new
                {
                    StaffRotationOrderId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    PositionId = table.Column<int>(type: "int", nullable: false),
                    StaffId = table.Column<int>(type: "int", nullable: false),
                    QueuePosition = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffRotationOrders", x => x.StaffRotationOrderId);
                    table.ForeignKey(
                        name: "FK_StaffRotationOrders_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "BranchId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StaffRotationOrders_StaffPositions_PositionId",
                        column: x => x.PositionId,
                        principalTable: "StaffPositions",
                        principalColumn: "PositionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StaffRotationOrders_Staffs_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Staffs",
                        principalColumn: "StaffId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RosterShifts_BranchId",
                table: "RosterShifts",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_RosterShifts_PositionId",
                table: "RosterShifts",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_RosterShifts_StaffId",
                table: "RosterShifts",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffRotationOrders_BranchId",
                table: "StaffRotationOrders",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffRotationOrders_PositionId",
                table: "StaffRotationOrders",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffRotationOrders_StaffId",
                table: "StaffRotationOrders",
                column: "StaffId");

            migrationBuilder.AddForeignKey(
                name: "FK_Rosters_Branches_BranchId",
                table: "Rosters",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "BranchId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RosterShifts_Branches_BranchId",
                table: "RosterShifts",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "BranchId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RosterShifts_StaffPositions_PositionId",
                table: "RosterShifts",
                column: "PositionId",
                principalTable: "StaffPositions",
                principalColumn: "PositionId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RosterShifts_Staffs_StaffId",
                table: "RosterShifts",
                column: "StaffId",
                principalTable: "Staffs",
                principalColumn: "StaffId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rosters_Branches_BranchId",
                table: "Rosters");

            migrationBuilder.DropForeignKey(
                name: "FK_RosterShifts_Branches_BranchId",
                table: "RosterShifts");

            migrationBuilder.DropForeignKey(
                name: "FK_RosterShifts_StaffPositions_PositionId",
                table: "RosterShifts");

            migrationBuilder.DropForeignKey(
                name: "FK_RosterShifts_Staffs_StaffId",
                table: "RosterShifts");

            migrationBuilder.DropTable(
                name: "StaffRotationOrders");

            migrationBuilder.DropIndex(
                name: "IX_RosterShifts_BranchId",
                table: "RosterShifts");

            migrationBuilder.DropIndex(
                name: "IX_RosterShifts_PositionId",
                table: "RosterShifts");

            migrationBuilder.DropIndex(
                name: "IX_RosterShifts_StaffId",
                table: "RosterShifts");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "RosterShifts");

            migrationBuilder.DropColumn(
                name: "PositionId",
                table: "RosterShifts");

            migrationBuilder.DropColumn(
                name: "StaffId",
                table: "RosterShifts");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Rosters");

            migrationBuilder.DropColumn(
                name: "WeekEndDate",
                table: "Rosters");

            migrationBuilder.RenameColumn(
                name: "WorkDate",
                table: "RosterShifts",
                newName: "ShiftDate");

            migrationBuilder.CreateTable(
                name: "Certifications",
                columns: table => new
                {
                    CertificationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CertificationName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Certifications", x => x.CertificationId);
                });

            migrationBuilder.CreateTable(
                name: "HRNotifications",
                columns: table => new
                {
                    NotificationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RosterShiftId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HRNotifications", x => x.NotificationId);
                    table.ForeignKey(
                        name: "FK_HRNotifications_RosterShifts_RosterShiftId",
                        column: x => x.RosterShiftId,
                        principalTable: "RosterShifts",
                        principalColumn: "RosterShiftId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ShiftRequirements",
                columns: table => new
                {
                    ShiftRequirementId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PositionId = table.Column<int>(type: "int", nullable: false),
                    RosterShiftId = table.Column<int>(type: "int", nullable: false),
                    FilledCount = table.Column<int>(type: "int", nullable: false),
                    NumberOfStaffNeeded = table.Column<int>(type: "int", nullable: false),
                    RequiredCertificationIds = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequiredSkillIds = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShiftRequirements", x => x.ShiftRequirementId);
                    table.ForeignKey(
                        name: "FK_ShiftRequirements_RosterShifts_RosterShiftId",
                        column: x => x.RosterShiftId,
                        principalTable: "RosterShifts",
                        principalColumn: "RosterShiftId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ShiftRequirements_StaffPositions_PositionId",
                        column: x => x.PositionId,
                        principalTable: "StaffPositions",
                        principalColumn: "PositionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ShiftTemplates",
                columns: table => new
                {
                    ShiftTemplateId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    PositionId = table.Column<int>(type: "int", nullable: false),
                    ActiveDays = table.Column<int>(type: "int", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    NumberOfStaffNeeded = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShiftTemplates", x => x.ShiftTemplateId);
                    table.ForeignKey(
                        name: "FK_ShiftTemplates_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "BranchId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ShiftTemplates_StaffPositions_PositionId",
                        column: x => x.PositionId,
                        principalTable: "StaffPositions",
                        principalColumn: "PositionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Skills",
                columns: table => new
                {
                    SkillId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SkillName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Skills", x => x.SkillId);
                });

            migrationBuilder.CreateTable(
                name: "StaffAvailabilitySlots",
                columns: table => new
                {
                    AvailabilitySlotId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StaffId = table.Column<int>(type: "int", nullable: false),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffAvailabilitySlots", x => x.AvailabilitySlotId);
                    table.ForeignKey(
                        name: "FK_StaffAvailabilitySlots_Staffs_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Staffs",
                        principalColumn: "StaffId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StaffExperiences",
                columns: table => new
                {
                    StaffExperienceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PositionId = table.Column<int>(type: "int", nullable: false),
                    StaffId = table.Column<int>(type: "int", nullable: false),
                    MonthsExperience = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffExperiences", x => x.StaffExperienceId);
                    table.ForeignKey(
                        name: "FK_StaffExperiences_StaffPositions_PositionId",
                        column: x => x.PositionId,
                        principalTable: "StaffPositions",
                        principalColumn: "PositionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StaffExperiences_Staffs_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Staffs",
                        principalColumn: "StaffId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StaffLeaves",
                columns: table => new
                {
                    LeaveId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StaffId = table.Column<int>(type: "int", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffLeaves", x => x.LeaveId);
                    table.ForeignKey(
                        name: "FK_StaffLeaves_Staffs_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Staffs",
                        principalColumn: "StaffId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StaffCertifications",
                columns: table => new
                {
                    StaffCertificationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CertificationId = table.Column<int>(type: "int", nullable: false),
                    StaffId = table.Column<int>(type: "int", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffCertifications", x => x.StaffCertificationId);
                    table.ForeignKey(
                        name: "FK_StaffCertifications_Certifications_CertificationId",
                        column: x => x.CertificationId,
                        principalTable: "Certifications",
                        principalColumn: "CertificationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StaffCertifications_Staffs_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Staffs",
                        principalColumn: "StaffId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ShiftCandidates",
                columns: table => new
                {
                    ShiftCandidateId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShiftRequirementId = table.Column<int>(type: "int", nullable: false),
                    StaffId = table.Column<int>(type: "int", nullable: false),
                    OfferedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RankOrder = table.Column<int>(type: "int", nullable: false),
                    RankScore = table.Column<double>(type: "float", nullable: false),
                    RespondedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShiftCandidates", x => x.ShiftCandidateId);
                    table.ForeignKey(
                        name: "FK_ShiftCandidates_ShiftRequirements_ShiftRequirementId",
                        column: x => x.ShiftRequirementId,
                        principalTable: "ShiftRequirements",
                        principalColumn: "ShiftRequirementId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ShiftCandidates_Staffs_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Staffs",
                        principalColumn: "StaffId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StaffSkills",
                columns: table => new
                {
                    StaffSkillId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SkillId = table.Column<int>(type: "int", nullable: false),
                    StaffId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffSkills", x => x.StaffSkillId);
                    table.ForeignKey(
                        name: "FK_StaffSkills_Skills_SkillId",
                        column: x => x.SkillId,
                        principalTable: "Skills",
                        principalColumn: "SkillId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StaffSkills_Staffs_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Staffs",
                        principalColumn: "StaffId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HRNotifications_RosterShiftId",
                table: "HRNotifications",
                column: "RosterShiftId");

            migrationBuilder.CreateIndex(
                name: "IX_ShiftCandidates_ShiftRequirementId",
                table: "ShiftCandidates",
                column: "ShiftRequirementId");

            migrationBuilder.CreateIndex(
                name: "IX_ShiftCandidates_StaffId",
                table: "ShiftCandidates",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_ShiftRequirements_PositionId",
                table: "ShiftRequirements",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_ShiftRequirements_RosterShiftId",
                table: "ShiftRequirements",
                column: "RosterShiftId");

            migrationBuilder.CreateIndex(
                name: "IX_ShiftTemplates_BranchId",
                table: "ShiftTemplates",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_ShiftTemplates_PositionId",
                table: "ShiftTemplates",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffAvailabilitySlots_StaffId",
                table: "StaffAvailabilitySlots",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffCertifications_CertificationId",
                table: "StaffCertifications",
                column: "CertificationId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffCertifications_StaffId",
                table: "StaffCertifications",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffExperiences_PositionId",
                table: "StaffExperiences",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffExperiences_StaffId",
                table: "StaffExperiences",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffLeaves_StaffId",
                table: "StaffLeaves",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffSkills_SkillId",
                table: "StaffSkills",
                column: "SkillId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffSkills_StaffId",
                table: "StaffSkills",
                column: "StaffId");

            migrationBuilder.AddForeignKey(
                name: "FK_Rosters_Branches_BranchId",
                table: "Rosters",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "BranchId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
