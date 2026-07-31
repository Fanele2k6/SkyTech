using RecruitmentSystem.Models;

namespace RecruitmentSystem.ViewModels
{
    public class AssessmentManagementViewModel
    {
        public Assessment Assessment { get; set; } = new Assessment();

        public int QuestionCount { get; set; }
    }
}