using static SkyTech.Models.JobPostServices.By_TRENCH.Enums.StaffAnalysisEnums;

namespace SkyTech.Models.JobPostServices.By_TRENCH
{
    public class StaffingAnalysisRun
    {
        public int Id { get; set; }

        public int? ScheduleId { get; set; }
        public StaffingAnalysisSchedule Schedule { get; set; }

        public AnalysisTrigger Trigger { get; set; }

        public int? TriggeredByAdminId { get; set; }
        public SystemAdmin TriggeredByAdmin { get; set; }

        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }

        public AnalysisRunStatus Status { get; set; } = AnalysisRunStatus.Pending;
        public string ErrorMessage { get; set; } = "";

        public int DepartmentsAnalyzed { get; set; }
        public int UnderstaffedPositionsFound { get; set; }
        public int RecommendationsGenerated { get; set; }

        public ICollection<DepartmentStaffingSnapshot> Snapshots { get; set; }
        public ICollection<JobVacancyRecommendation> Recommendations { get; set; }
    }
}
