namespace SkyTech.Models.JobPostServices.By_TRENCH
{
    public class Department
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Position> Positions { get; set; }
        public ICollection<Staff> Staffs { get; set; }
    }

}
