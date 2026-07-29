using static SkyTech.Models.JobPostServices.By_TRENCH.Enums.StaffAnalysisEnums;

namespace SkyTech.Models.JobPostServices.By_TRENCH
{
    public class StaffingAnalysisSchedule
    {
        public int Id { get; set; }

        public int SystemAdminId { get; set; }
        public SystemAdmin? SystemAdmin { get; set; }

        public AnalysisFrequency Frequency { get; set; }
        public string CronExpression { get; set; } = ""; // Only for Custom frequency
        public TimeSpan TimeOfDay { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? LastRunAt { get; set; }
        public DateTime? NextRunAt { get; set; }

        public ICollection<StaffingAnalysisRun> Runs { get; set; }= new List<StaffingAnalysisRun>();
    }
}
