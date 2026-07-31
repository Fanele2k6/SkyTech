namespace RecruitmentSystem.ViewModels
{
    public class HRReviewViewModel
    {
        public int ApplicationId { get; set; }

        public string ApplicantName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string JobTitle { get; set; } = string.Empty;

        public int Score { get; set; }

        public bool Passed { get; set; }

        public string RecruitmentStatus { get; set; } = string.Empty;

        public bool HasPublishedAssessment { get; set; }
    }
}
