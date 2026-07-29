using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkyTech.JobPostServices;
using SkyTech.Models;
using SkyTech.Models.JobPostServices;

namespace SkyTech.Controllers
{
    public class CustomerApplicationsController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly ICvExtractionService _cvExtractionService;
        public CustomerApplicationsController(ApplicationDbContext context, ICvExtractionService cvExtractionService)
        {
            db = context ?? throw new ArgumentNullException(nameof(context));
            _cvExtractionService = cvExtractionService;
        }
        public async Task<IActionResult> Careers()
        {
            var listOfJobs = await db.JobPosts 
                .ToListAsync();
            return View(listOfJobs);

        }


        [HttpGet]
        public async Task<IActionResult> StartApplication(int postId)
        {
            var job = await db.JobPosts.FindAsync(postId);
            if (job == null)
                return NotFound();

            return View(job);
        }

        [HttpPost]
        public async Task<IActionResult> UploadCv(int postId, IFormFile cvFile)
        {
            var job = await db.JobPosts.FindAsync(postId);
            if (job == null) return NotFound();

            if (cvFile == null || cvFile.Length == 0)
            {
                ModelState.AddModelError("", "Please select a CV file to upload.");
                return View("StartApplication", job);
            }

            var extension = Path.GetExtension(cvFile.FileName).ToLowerInvariant();
            if (extension != ".pdf" && extension != ".docx")
            {
                ModelState.AddModelError("", "Only PDF or DOCX files are supported.");
                return View("StartApplication", job);
            }

            string cvText;
            try
            {
                cvText = CvTextExtractor.ExtractText(cvFile);
            }
            catch
            {
                ModelState.AddModelError("", "We couldn't read that file. Please try a different CV.");
                return View("StartApplication", job);
            }

            CvExtractionResult extracted;
            try
            {
                extracted = await _cvExtractionService.ExtractFromTextAsync(cvText);
            }
            catch (Exception ex)
            {
                extracted = new CvExtractionResult();
                // TEMPORARY — remove once working
                TempData["cvError"] = $"CV extraction failed: {ex.Message}";
            }

            //CvExtractionResult extracted;
            //try
            //{
            //    extracted = await _cvExtractionService.ExtractFromTextAsync(cvText);
            //}
            //catch
            //{
            //    // AI extraction failed — fall back to a blank form so the applicant
            //    // can still type everything in manually rather than being blocked
            //    extracted = new CvExtractionResult();
            //    TempData["cvError"] = "We couldn't auto-fill your details — please enter them manually.";
            //}

            var applicationForm = new ApplicationForm
            {
                PostId = postId,
                FirstName = extracted.FirstName,
                LastName = extracted.LastName,
                IDNumber = extracted.IDNumber,
                Email = extracted.Email,
                Location = extracted.Location,
                Experience = extracted.Experience,
                Skills = extracted.Skills,
                Links = extracted.Links
            };

            return View("ReviewApplication", applicationForm);
        }

        public IActionResult ReviewApplication()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitApplication(ApplicationForm model)
        {
            int? customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
                return RedirectToAction("Login", "Authentication");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return Content(string.Join("\n", errors));
            }

            // Re-check the job still exists and hasn't been pulled since the customer started applying
            var job = await db.JobPosts.FindAsync(model.PostId);
            if (job == null)
                return NotFound();

            model.CustomerId = customerId.Value;

            db.ApplicationForms.Add(model);
            await db.SaveChangesAsync();

            TempData["successful"] = "Your application has been submitted successfully";
            return RedirectToAction("JobPosts", "CustomerApplications"); // or wherever your listing view lives
        }
    }
}
