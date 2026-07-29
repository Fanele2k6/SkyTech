namespace SkyTech.Models.JobPostServices.By_TRENCH
{
    public class Position
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        public int DepartmentId { get; set; }
        public Department Department { get; set; }

        // HR-owned: max number of people allowed in this position
        public int MaxHeadcount { get; set; }

        public int? MaxHeadcountSetByStaffId { get; set; }
        public Staff MaxHeadcountSetByStaff { get; set; }
        public DateTime? MaxHeadcountUpdatedAt { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<Staff> Staffs { get; set; }
        public ICollection<JobRoleKnowledge> JobRoleKnowledges { get; set; }  // NEW
    }
}
