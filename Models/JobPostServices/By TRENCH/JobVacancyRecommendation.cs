using static SkyTech.Models.JobPostServices.By_TRENCH.Enums.StaffAnalysisEnums;

namespace SkyTech.Models.JobPostServices.By_TRENCH
{
    public class JobVacancyRecommendation
    {
        public int Id { get; set; }

        public int AnalysisRunId { get; set; }
        public StaffingAnalysisRun AnalysisRun { get; set; }

        public int? SnapshotId { get; set; }
        public DepartmentStaffingSnapshot Snapshot { get; set; }

        public int DepartmentId { get; set; }
        public Department Department { get; set; }

        public int PositionId { get; set; }
        public Position Position { get; set; }

        public string SuggestedJobTitle { get; set; }
        public string SuggestedResponsibilities { get; set; }      // AI-generated
        public string SuggestedRequiredCerts { get; set; }         // AI-generated
        public string SuggestedRequiredSkillsAndXP { get; set; }   // AI-generated

        public int RecommendedHeadcount { get; set; }

        public VacancyType VacancyType { get; set; } = VacancyType.Permanent;

        // Only set when VacancyType == TemporaryLeaveCover
        public int? LeaveRequestId { get; set; }
        public LeaveRequest LeaveRequest { get; set; }
        public DateTime? ContractStartDate { get; set; }
        public DateTime? ContractEndDate { get; set; }
        public int? ContractDurationDays { get; set; }

        public RecommendationStatus Status { get; set; } = RecommendationStatus.Draft;
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

        // UC2 review fields (nullable)
        public int? ReviewedByStaffId { get; set; }
        public Staff ReviewedByStaff { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string ReviewNotes { get; set; } = "";
        public DateTime? PublishedAt { get; set; }
    }
}
