using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace RecruitmentSystem.Models
{
    public class Application
    {
        [Key]
        public int ApplicationId { get; set; }
        public int ApplicantId { get; set; }
        public int JobId { get; set; }
        public DateTime DateApplied { get; set; }
        public string RecruitmentStatus { get; set; } = "Applied";
        public bool TestWritten { get; set; } = false;
        public int? TestScore { get; set; }

        [ForeignKey("ApplicantId")]
        public Applicant? Applicant { get; set; }

        [ForeignKey("JobId")]
        public Job? Job { get; set; }
        public TestResult? TestResult { get; set; }
    }
}
