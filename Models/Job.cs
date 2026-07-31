using System.ComponentModel.DataAnnotations;
namespace RecruitmentSystem.Models
{
    public class Job
    {
        [Key]
        public int JobId { get; set; }

        [Required]
        public string JobTitle { get; set; } = string.Empty;
        public string JobDescription { get; set; } = string.Empty;
        public DateTime ClosingDate { get; set; }
        public Assessment? Assessment { get; set; }

        public ICollection<Application>? Applications { get; set; }
    }
}
