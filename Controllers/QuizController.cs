using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecruitmentSystem.Data;
using RecruitmentSystem.ViewModels;

namespace RecruitmentSystem.Controllers
{
    public class QuizController : Controller
    {
        private readonly RecruitmentContext _context;

        public QuizController(RecruitmentContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            int? applicantId = HttpContext.Session.GetInt32("ApplicantId");

            if (applicantId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Find the applicant's application
            var application = _context.Applications
                .Include(a => a.Job)
                .FirstOrDefault(a => a.ApplicantId == applicantId.Value);

            if (application == null)
            {
                return Content("You have not applied for any vacancy.");
            }

            if (application.RecruitmentStatus != "Shortlisted")
            {
                return Content("You are not eligible to write this assessment.");
            }

            if (application.TestWritten)
            {
                return Content("You have already completed this assessment.");
            }

            // Find the published assessment for the job
            var assessment = _context.Assessments
                .Include(a => a.QuizQuestions)
                .FirstOrDefault(a =>
                    a.JobId == application.JobId &&
                    a.IsPublished);

            if (assessment == null)
            {
                return Content("No assessment has been published for this vacancy yet.");
            }

            var model = new QuizViewModel();

            foreach (var question in assessment.QuizQuestions)
            {
                model.Questions.Add(new QuizQuestionViewModel
                {
                    QuestionId = question.QuestionId,
                    QuestionText = question.QuestionText,
                    OptionA = question.OptionA,
                    OptionB = question.OptionB,
                    OptionC = question.OptionC,
                    OptionD = question.OptionD
                });
            }
            ViewBag.TimeLimit = assessment.TimeLimit;
            return View(model);
        }

        
        
        [HttpPost]
        public IActionResult SubmitTest(QuizViewModel model)
        {
            int? applicantId = HttpContext.Session.GetInt32("ApplicantId");

            if (applicantId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var application = _context.Applications
                .FirstOrDefault(a => a.ApplicantId == applicantId.Value);

            if (application == null)
            {
                return Content("Application not found.");
            }

            int score = 0;

            foreach (var answer in model.Questions)
            {
                var question = _context.QuizQuestions
                    .FirstOrDefault(q => q.QuestionId == answer.QuestionId);

                if (question != null &&
                    answer.SelectedAnswer == question.CorrectAnswer)
                {
                    score++;
                }
            }

            var assessment = _context.Assessments
            .FirstOrDefault(a => a.JobId == application.JobId);

            double percentage = (double)score / model.Questions.Count * 100;

            bool passed = percentage >= assessment!.PassMark;


            application.TestWritten = true;
            application.TestScore = score;

            application.RecruitmentStatus = passed
                ? "Passed Test"
                : "Failed Test";

            var result = new Models.TestResult
            {
                ApplicationId = application.ApplicationId,
                Score = score,
                Passed = passed,
                DateWritten = DateTime.Now
            };

            _context.TestResults.Add(result);

            _context.SaveChanges();

            var resultModel = new TestResultViewModel
            {
                Score = score,
                TotalQuestions = model.Questions.Count,
                Passed = passed,
                Message = passed
                    ? "Congratulations! You passed the assessment."
                    : "Unfortunately, you did not achieve the required pass mark."
            };
            

            return View("Result", resultModel);
        }
    }
}
