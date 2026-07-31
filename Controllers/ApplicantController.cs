using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecruitmentSystem.Data;
using RecruitmentSystem.ViewModels;

namespace RecruitmentSystem.Controllers
{
    public class ApplicantController : Controller
    {
        private readonly RecruitmentContext _context;

        public ApplicantController(RecruitmentContext context)
        {
            _context = context;
        }
        public IActionResult Dashboard()
        {
            int? applicantId = HttpContext.Session.GetInt32("ApplicantId");

            if (applicantId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Get the applicant's application together with the related Job and Applicant
            var application = _context.Applications
                .Include(a => a.Applicant)
                .Include(a => a.Job)
                .FirstOrDefault(a => a.ApplicantId == applicantId.Value);

            if (application == null)
            {
                return RedirectToAction("Index", "Apply");
            }

            // Check whether this job has a published assessment
            var assessment = _context.Assessments
                .FirstOrDefault(a =>
                    a.JobId == application.JobId &&
                    a.IsPublished);

            var model = new ApplicantDashboardViewModel
            {
                FullName = application.Applicant!.FullName,
                JobTitle = application.Job!.JobTitle,
                RecruitmentStatus = application.RecruitmentStatus,

                CanWriteTest =
                    application.RecruitmentStatus == "Shortlisted"
                    && !application.TestWritten
                    && assessment != null
            };

            return View(model);

        }
    }
}

