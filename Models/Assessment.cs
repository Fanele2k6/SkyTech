using System.ComponentModel.DataAnnotations;

namespace RecruitmentSystem.Models
{
    public class Assessment
    {
        [Key]
        public int AssessmentId { get; set; }

        public int JobId { get; set; }

        public string Title { get; set; } = "";

        public Job? Job { get; set; }
        public bool IsPublished { get; set; } = false;

        [Required]
        [Range(1, 100, ErrorMessage = "Pass mark must be between 1 and 100.")]
        public int PassMark { get; set; }

        [Required]
        [Range(1, 180)]
        public int TimeLimit { get; set; }

        public ICollection<QuizQuestion> QuizQuestions { get; set; } = new List<QuizQuestion>();
    }
}