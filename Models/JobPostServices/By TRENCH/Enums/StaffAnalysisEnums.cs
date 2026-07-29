namespace SkyTech.Models.JobPostServices.By_TRENCH.Enums
{
    public class StaffAnalysisEnums
    {
        public enum EmploymentStatus
        {
            Active,
            OnLeave,
            Resigned,
            Terminated
        }

        public enum LeaveStatus
        {
            Pending,
            Approved,
            Rejected,
            Cancelled
        }

        public enum AnalysisFrequency
        {
            Daily,
            Weekly,
            Monthly,
            Custom
        }

        public enum AnalysisRunStatus
        {
            Pending,
            Running,
            Completed,
            Failed
        }

        public enum AnalysisTrigger
        {
            Scheduled,
            ManualByAdmin
        }

        public enum VacancyType
        {
            Permanent,
            TemporaryLeaveCover
        }

        public enum RecommendationStatus
        {
            Draft,
            PendingHRReview,
            Approved,
            Rejected,
            Published
        }
    }
}
