using System.ComponentModel.DataAnnotations;
namespace RecruitmentSystem.Models
{
    public class TestResult
    {
        [Key]
        public int ResultId { get; set; }
        public int ApplicationId { get; set; }
        public int Score { get; set; }
        public bool Passed { get; set; }
        public DateTime DateWritten { get; set; }
        public Application? Application { get; set; }
    }
}
