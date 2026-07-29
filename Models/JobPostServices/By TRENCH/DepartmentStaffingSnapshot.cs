namespace SkyTech.Models.JobPostServices.By_TRENCH
{
    public class DepartmentStaffingSnapshot
    {
        public int Id { get; set; }

        public int AnalysisRunId { get; set; }
        public StaffingAnalysisRun AnalysisRun { get; set; }

        public int DepartmentId { get; set; }
        public Department Department { get; set; }

        public int PositionId { get; set; }
        public Position Position { get; set; }

        public int RequiredHeadcount { get; set; }
        public int ActiveEmployeeCount { get; set; }
        public int OnLeaveCount { get; set; }
        public int ResignedRecentCount { get; set; }

        public int VacancyCount { get; set; }
        public bool IsUnderstaffed { get; set; }

        public ICollection<JobVacancyRecommendation> Recommendations { get; set; }
    }
}
