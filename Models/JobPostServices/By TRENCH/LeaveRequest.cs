using static SkyTech.Models.JobPostServices.By_TRENCH.Enums.StaffAnalysisEnums;

namespace SkyTech.Models.JobPostServices.By_TRENCH
{
    public class LeaveRequest
    {
        public int Id { get; set; }

        public int StaffId { get; set; }
        public Staff Staff { get; set; }

        public int LeaveTypeId { get; set; }
        public LeaveType LeaveType { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Reason { get; set; }

        public LeaveStatus Status { get; set; } = LeaveStatus.Pending;
        public DateTime AppliedOn { get; set; } = DateTime.UtcNow;

        public int? ApprovedByStaffId { get; set; }
        public Staff ApprovedByStaff { get; set; }
        public DateTime? ApprovedOn { get; set; }

        // Links to temporary job vacancy recommendations this leave triggered
        public ICollection<JobVacancyRecommendation> TemporaryCoverRecommendations { get; set; }
    }
}
