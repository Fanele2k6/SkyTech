using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SkyTech.Models;
using SkyTech.Models.JobPostServices;
using SkyTech.Services;

namespace SkyTech.Controllers
{
    [AuthorizeUser]
    public class HRController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly EmailServices _services;
        private readonly IAIJobPostDraftService _aiService;  
        private readonly IWebHostEnvironment _webHostEnvironment;

        public HRController(ApplicationDbContext context, EmailServices services, IAIJobPostDraftService aiService, IWebHostEnvironment webHostEnvironment)
        {
            db = context;
            _services = services;
            _aiService = aiService;
            _webHostEnvironment = webHostEnvironment;
        }

        //==============================HR POSTING AND REVIEWING JOBS==============================
        public IActionResult JobCenter()
        {
            return View("ManagingJobs/JobCenter");
        }

        public IActionResult ComposeJob()
        {
            return View("Managingjobs/ComposeJob");
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> ComposeJob(JobPost model)
        {
            int currentId = HttpContext.Session.GetInt32("UserId")!.Value;
            //validation
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);

                return Content(string.Join("\n", errors));
            }
            //if (!ModelState.IsValid)
            //{
            //    return View("ManagingJobs/ComposeJob", model);
            //    //return View(model);
            //}

            model.HRId = currentId;
            model.PostStatus = CurrentPostStatus.Published;
            model.CreatedAt = DateTime.Now;
            model.UpdatedAt = DateTime.Now;

            db.Add(model);
            await db.SaveChangesAsync();
            TempData["successful"] = "Job Posted successfully";
            return RedirectToAction("JobCenter");

        }


        // ---------- AI-ASSISTED POST ----------

        //================================================================================================
        public async Task<IActionResult> ComposeJobAI(int roleId)
        {
            var role = await db.JobRoleKnowledges.FindAsync(roleId);
            if (role == null) return NotFound();

            ViewBag.RoleId = role.RoleId;
            ViewBag.JobTitle = role.JobTitle;
            return View("ManagingJobs/ComposeJobAI");
        }


        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> GenerateJobDraft(int roleId, JobPost model)
        {
            int currentId = HttpContext.Session.GetInt32("UserId")!.Value;
            var role = await db.JobRoleKnowledges.FindAsync(roleId);
            if (role == null) return NotFound();

            JobPostDraftResult draft;
            try
            {
                draft = await _aiService.GenerateDraftFromRoleAsync(role);
            }
            catch
            {
                draft = new JobPostDraftResult
                {
                    Responsibilities = role.CompanyResponsibilities,
                    RequiredCerts = role.RequiredQualifications,
                    RequiredSkillsAndXP = role.RequiredSkills + "\n" + role.RequiredExperience
                };
                TempData["aiError"] = "AI formatting failed — showing the raw approved information instead.";
            }

            model.HRId = currentId;
            model.JobTitle = role.JobTitle;
            model.Responsibilities = draft.Responsibilities;
            model.RequiredCerts = draft.RequiredCerts;
            model.RequiredSkillsAndXP = draft.RequiredSkillsAndXP;
            model.PostStatus = CurrentPostStatus.Draft;
            model.CreatedAt = DateTime.Now;
            model.UpdatedAt = DateTime.Now;

            db.Add(model);
            await db.SaveChangesAsync();
            return RedirectToAction("ReviewJobDraft", new { id = model.PostId });
        }

        // Step 3: owner reviews/edits the AI draft
        [HttpGet]
        public async Task<IActionResult> ReviewJobDraft(int id)
        {
            var post = await db.JobPosts.FindAsync(id);
            if (post == null) return NotFound();
            return View("ManagingJobs/ReviewJobDraft", post);
        }

        // Step 4a: owner edits text then clicks Publish
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> PublishJobDraft(JobPost model)
        {
            var post = await db.JobPosts.FindAsync(model.PostId);
            if (post == null) return NotFound();

            post.Responsibilities = model.Responsibilities;
            post.RequiredCerts = model.RequiredCerts;
            post.RequiredSkillsAndXP = model.RequiredSkillsAndXP;
            post.PostStatus = CurrentPostStatus.Published;
            post.UpdatedAt = DateTime.Now;

            await db.SaveChangesAsync();
            TempData["successful"] = "Job Posted successfully";
            return RedirectToAction("JobCenter");
        }

        // Step 4b: owner isn't happy with the draft, asks AI to try again
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> RegenerateJobDraft(int id)
        {
            var post = await db.JobPosts.FindAsync(id);
            if (post == null) return NotFound();

            var draft = await _aiService.GenerateDraftAsync(new JobPostDraftRequest
            {
                JobTitle = post.JobTitle,
                Department = post.Department.ToString(),
                Location = post.Location.ToString(),
                EmploymentType = post.EmploymentType.ToString(),
                SalaryMin = post.SalaryMin,
                SalaryMax = post.SalaryMax
            });

            post.Responsibilities = draft.Responsibilities;
            post.RequiredCerts = draft.RequiredCerts;
            post.RequiredSkillsAndXP = draft.RequiredSkillsAndXP;
            post.UpdatedAt = DateTime.Now;

            await db.SaveChangesAsync();
            return RedirectToAction("ReviewJobDraft", new { id });
        }

        // GET: dropdown of active roles or roles that the company stil need workers for
        public IActionResult SelectJobRole()
        {
            var roles = db.JobRoleKnowledges
                .Where(r => r.IsActive)
                .OrderBy(r => r.JobTitle)
                .ToList();

            ViewBag.Roles = new SelectList(roles, "RoleId", "JobTitle");
            return View("ManagingJobs/SelectJobRole");
        }

        // POST: owner picked a role and a mode (manual or AI)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SelectJobRole(int roleId, string mode)
        {
            var role = await db.JobRoleKnowledges.FindAsync(roleId);
            if (role == null) return NotFound();

            if (mode == "manual")
            {
                // prefill the existing manual ComposeJob form with approved info
                var seeded = new JobPost
                {
                    JobTitle = role.JobTitle,
                    Responsibilities = role.CompanyResponsibilities,
                    RequiredCerts = role.RequiredQualifications,
                    RequiredSkillsAndXP = role.RequiredSkills + "\n" + role.RequiredExperience
                };
                return View("ManagingJobs/ComposeJob", seeded);
            }

            // AI mode — go collect the remaining seed fields (location, salary, etc.)
            return RedirectToAction("ComposeJobAI", new { roleId = role.RoleId });
        }

       //============================HR managing job applicationd====================

        public IActionResult HRDash()
        {
            return View();
        }

        //HR review Application
        public async Task<IActionResult> HRReviewApplication()
        {
            int? currentId = HttpContext.Session.GetInt32("UserId");

            var jobApplications = await db.ApplicationForms
                .ToListAsync();

            return View(jobApplications);
        }

        public async Task<IActionResult> ChangeStatus(int id, JobApplicationStat status)
        {
            var application = await db.ApplicationForms.FindAsync(id);
            if (application == null) return NotFound();

            application.JobStat = status;
            await db.SaveChangesAsync();

            if (status == JobApplicationStat.Rejected)
            {
                _services.RejectJobApplication(application.Email, application.FirstName);

            }
            else if(status == JobApplicationStat.Online_interview)
            {
                _services.OnlineInterview(application.Email, application.FirstName);

            }
            else
            {
                _services.PhysicalInterview(application.Email, application.FirstName);

            }
            TempData["Message"] = "Feedback Sent";
            return RedirectToAction("HRDash");
        }
    }
}
