using Microsoft.EntityFrameworkCore;
using RecruitmentSystem.Models;
namespace RecruitmentSystem.Data
{
    public class RecruitmentContext : DbContext
    {
        public RecruitmentContext(DbContextOptions<RecruitmentContext> options)
            : base(options)
        {

        }
        public DbSet<Applicant> Applicants { get; set; }

        public DbSet<Job> Jobs { get; set; }

        public DbSet<Application> Applications { get; set; }

        public DbSet<QuizQuestion> QuizQuestions { get; set; }

        public DbSet<TestResult> TestResults { get; set; }
        public DbSet<Assessment> Assessments { get; set; }
    }
}
