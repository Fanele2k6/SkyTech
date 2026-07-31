using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RecruitmentSystem.Data;
using RecruitmentSystem.Models;

namespace RecruitmentSystem.Controllers
{
    public class ApplyController : Controller
    {
        private readonly RecruitmentContext _context;

        public ApplyController(RecruitmentContext context)
        {
            _context = context;
        }

        // Display available jobs
        public IActionResult Index()
        {
            int? applicantId = HttpContext.Session.GetInt32("ApplicantId");

            if (applicantId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Has the applicant already applied?
            bool hasApplied = _context.Applications
                .Any(a => a.ApplicantId == applicantId.Value);

            if (hasApplied)
            {
                return RedirectToAction("Dashboard", "Applicant");
            }

            ViewBag.Jobs = new SelectList(
                _context.Jobs
                    .Where(j => j.ClosingDate >= DateTime.Today)
                    .ToList(),
                "JobId",
                "JobTitle");

            return View();
        }

        [HttpPost]
        public IActionResult Apply(int jobId)
        {
            int? applicantId = HttpContext.Session.GetInt32("ApplicantId");

            if (applicantId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var application = new Application
            {
                ApplicantId = applicantId.Value,
                JobId = jobId,
                DateApplied = DateTime.Now,
                RecruitmentStatus = "Applied",
                TestWritten = false
            };

            _context.Applications.Add(application);
            _context.SaveChanges();

            return RedirectToAction("Dashboard", "Applicant");
        }
    }
}
