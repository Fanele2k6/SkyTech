namespace RecruitmentSystem.ViewModels
{
    public class ApplicantDashboardViewModel
    {
        public string FullName { get; set; } = string.Empty;

        public string JobTitle { get; set; } = string.Empty;

        public string RecruitmentStatus { get; set; } = string.Empty;

        public bool CanWriteTest { get; set; }
    }
}
