namespace SkyTech.Models.JobPostServices.By_TRENCH
{
    public class LeaveType
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int MaxDaysPerYear { get; set; }
        public bool RequiresApproval { get; set; } = true;
        public bool IsPaid { get; set; } = true;
        public string Description { get; set; }

        public ICollection<LeaveRequest> LeaveRequests { get; set; }
    }
}
