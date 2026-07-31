using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecruitmentSystem.Data;
using RecruitmentSystem.Models;
using RecruitmentSystem.ViewModels;

namespace RecruitmentSystem.Controllers
{
    public class HRController : Controller
    {
        private readonly RecruitmentContext _context;

        public HRController(RecruitmentContext context)
        {
            _context = context;
        }

        public IActionResult Index(string? search, int? jobId, string? status)
        {
            var query = _context.Applications
            .Include(a => a.Applicant)
            .Include(a => a.Job)
            .Include(a => a.TestResult)
            .AsQueryable();

            // Search by applicant name or email
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(a =>
                    a.Applicant!.FullName.Contains(search) ||
                    a.Applicant.Email.Contains(search));
            }

            // Filter by vacancy
            if (jobId.HasValue)
            {
                query = query.Where(a => a.JobId == jobId.Value);
            }

            // Filter by recruitment status
            if (!string.IsNullOrWhiteSpace(status) && status != "All")
            {
                query = query.Where(a => a.RecruitmentStatus == status);
            }

            var applicants = query
                .OrderBy(a => a.RecruitmentStatus)
                .ThenBy(a => a.Applicant!.FullName)
                .ToList();

            var model = applicants.Select(a => new HRReviewViewModel
            {
                ApplicationId = a.ApplicationId,
                ApplicantName = a.Applicant!.FullName,
                Email = a.Applicant.Email,
                JobTitle = a.Job!.JobTitle,
                Score = a.TestResult?.Score ?? 0,
                Passed = a.TestResult?.Passed ?? false,
                RecruitmentStatus = a.RecruitmentStatus,

                HasPublishedAssessment = _context.Assessments.Any(x =>
                    x.JobId == a.JobId &&
                    x.IsPublished)
            }).ToList();

            // Jobs for the vacancy filter
            ViewBag.Jobs = _context.Jobs
                .OrderBy(j => j.JobTitle)
                .ToList();

            // Statuses for the status filter
            ViewBag.Statuses = new List<string>
            {
                "Applied",
                "Shortlisted",
                "Passed Test",
                "Failed Test",
                "Interview Invited",
                "Rejected"
            };

            // Keep the current filter values
            ViewBag.Search = search;
            ViewBag.JobId = jobId;
            ViewBag.Status = status;

            return View(model);

        }


        public IActionResult Confirm(int id)
        {
            var application = _context.Applications
                .Include(a => a.Applicant)
                .FirstOrDefault(a => a.ApplicationId == id);

            if (application == null)
            {
                return NotFound();
            }

            // Cannot interview rejected applicants
            if (application.RecruitmentStatus == "Rejected")
            {
                TempData["Message"] = "Rejected applicants cannot be invited for an interview.";
                return RedirectToAction("Index");
            }

            // Applicant must be shortlisted first
            if (application.RecruitmentStatus == "Applied")
            {
                TempData["Message"] = "Shortlist the applicant before inviting them for an interview.";
                return RedirectToAction("Index");
            }

            // Check if this job has a published assessment
            bool hasAssessment = _context.Assessments
                .Any(a => a.JobId == application.JobId && a.IsPublished);

            // If a quiz exists, applicant must pass it
            if (hasAssessment && application.RecruitmentStatus != "Passed Test")
            {
                TempData["Message"] =
                    "This vacancy requires an assessment. The applicant must pass before an interview can be scheduled.";

                return RedirectToAction("Index");
            }

            application.RecruitmentStatus = "Interview Invited";

            _context.SaveChanges();

            TempData["Message"] =
                $"Interview invitation sent to {application.Applicant!.Email}";

            return RedirectToAction("Index");
        }

        // Display Add Vacancy page
        public IActionResult CreateJob()
        {
            return View();
        }

        // Save Vacancy
        [HttpPost]
        public IActionResult CreateJob(Job job)
        {
            if (!ModelState.IsValid)
            {
                return View(job);
            }

            _context.Jobs.Add(job);
            _context.SaveChanges();

            TempData["Message"] = "Vacancy added successfully.";

            return RedirectToAction("CreateJob");
        }

        // Shortlist applicant
        public IActionResult Shortlist(int id)
        {
            var application = _context.Applications
                .FirstOrDefault(a => a.ApplicationId == id);

            if (application == null)
            {
                return NotFound();
            }

            application.RecruitmentStatus = "Shortlisted";

            _context.SaveChanges();

            return RedirectToAction("Index");
        }


        // Reject applicant
        public IActionResult Reject(int id)
        {
            var application = _context.Applications
                .FirstOrDefault(a => a.ApplicationId == id);

            if (application == null)
            {
                return NotFound();
            }

            application.RecruitmentStatus = "Rejected";

            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult ManageAssessments()
        {
            var assessments = _context.Assessments
                .Include(a => a.Job)
                .Include(a => a.QuizQuestions)
                .Select(a => new AssessmentListViewModel
                {
                    AssessmentId = a.AssessmentId,
                    AssessmentTitle = a.Title,
                    JobTitle = a.Job!.JobTitle,
                    QuestionCount = a.QuizQuestions.Count,
                    PassMark = a.PassMark,
                    TimeLimit = a.TimeLimit,
                    IsPublished = a.IsPublished
                })
                .ToList();

            return View(assessments);
        }
        public IActionResult ManageAssessment(int id)
        {
            var assessment = _context.Assessments
                .Include(a => a.Job)
                .Include(a => a.QuizQuestions)
                .FirstOrDefault(a => a.AssessmentId == id);

            if (assessment == null)
            {
                return NotFound();
            }

            var model = new AssessmentManagementViewModel
            {
                Assessment = assessment,
                QuestionCount = assessment.QuizQuestions.Count
            };

            return View(model);
        }

        // Edit Assessment - display page
        public IActionResult EditAssessment(int id)
        {
            var assessment = _context.Assessments
                .FirstOrDefault(a => a.AssessmentId == id);

            if (assessment == null)
            {
                return NotFound();
            }

            // Published assessments cannot be edited
            if (assessment.IsPublished)
            {
                TempData["Message"] =
                    "Published assessments cannot be edited. Unpublish the assessment first.";

                return RedirectToAction("ManageAssessment", new { id });
            }

            var model = new CreateAssessmentViewModel
            {
                JobId = assessment.JobId,
                Title = assessment.Title,
                PassMark = assessment.PassMark,
                TimeLimit = assessment.TimeLimit
            };

            return View(model);
        }

        // Edit Assessment - save changes
        [HttpPost]
        public IActionResult EditAssessment(int id, CreateAssessmentViewModel model)
        {
            var assessment = _context.Assessments
                .FirstOrDefault(a => a.AssessmentId == id);

            if (assessment == null)
            {
                return NotFound();
            }

            // Published assessments cannot be edited
            if (assessment.IsPublished)
            {
                TempData["Message"] =
                    "Published assessments cannot be edited. Unpublish the assessment first.";

                return RedirectToAction("ManageAssessment", new { id });
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            assessment.Title = model.Title;
            assessment.PassMark = model.PassMark;
            assessment.TimeLimit = model.TimeLimit;

            _context.SaveChanges();

            TempData["Message"] = "Assessment updated successfully.";

            return RedirectToAction("ManageAssessment", new { id });
        }
        // Display Create Assessment page
        public IActionResult CreateAssessment()
        {
            var model = new CreateAssessmentViewModel();

            model.Jobs = _context.Jobs
                .Select(j => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = j.JobId.ToString(),
                    Text = j.JobTitle
                })
                .ToList();

            return View(model);
        }

        [HttpPost]
        public IActionResult CreateAssessment(CreateAssessmentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Jobs = _context.Jobs
                    .Select(j => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                    {
                        Value = j.JobId.ToString(),
                        Text = j.JobTitle
                    })
                    .ToList();

                return View(model);
            }

            // Check if this job already has an assessment
            var assessment = _context.Assessments
                .FirstOrDefault(a => a.JobId == model.JobId);

            // If not, create one
            if (assessment == null)
            {
                assessment = new Assessment
                {
                    JobId = model.JobId,
                    Title = model.Title,
                    PassMark = model.PassMark,
                    TimeLimit = model.TimeLimit,
                    IsPublished = false
                };

                _context.Assessments.Add(assessment);
                _context.SaveChanges();
            }

            // Go to Add Question page
            return RedirectToAction("AddQuestion", new
            {
                id = assessment.AssessmentId
            });
        }

        public IActionResult AddQuestion(int id)
        {
            var assessment = _context.Assessments
                .Include(a => a.QuizQuestions)
                .FirstOrDefault(a => a.AssessmentId == id);

            if (assessment == null)
            {
                return NotFound();
            }

            var model = new AddQuestionViewModel
            {
                AssessmentId = assessment.AssessmentId,
                AssessmentTitle = assessment.Title,
                ExistingQuestions = assessment.QuizQuestions!.ToList()
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult AddQuestion(AddQuestionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var question = new QuizQuestion
            {
                AssessmentId = model.AssessmentId,
                QuestionText = model.QuestionText,
                OptionA = model.OptionA,
                OptionB = model.OptionB,
                OptionC = model.OptionC,
                OptionD = model.OptionD,
                CorrectAnswer = model.CorrectAnswer
            };

            _context.QuizQuestions.Add(question);
            _context.SaveChanges();

            return RedirectToAction("AddQuestion",
                new { id = model.AssessmentId });
        }
        // Edit Question - display page
        public IActionResult EditQuestion(int id)
        {
            var question = _context.QuizQuestions
                .FirstOrDefault(q => q.QuestionId == id);

            if (question == null)
            {
                return NotFound();
            }

            var model = new AddQuestionViewModel
            {
                AssessmentId = question.AssessmentId,
                QuestionText = question.QuestionText,
                OptionA = question.OptionA,
                OptionB = question.OptionB,
                OptionC = question.OptionC,
                OptionD = question.OptionD,
                CorrectAnswer = question.CorrectAnswer
            };

            return View(model);
        }


        // Edit Question - save changes
        [HttpPost]
        public IActionResult EditQuestion(int id, AddQuestionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var question = _context.QuizQuestions
                .FirstOrDefault(q => q.QuestionId == id);

            if (question == null)
            {
                return NotFound();
            }

            question.QuestionText = model.QuestionText;
            question.OptionA = model.OptionA;
            question.OptionB = model.OptionB;
            question.OptionC = model.OptionC;
            question.OptionD = model.OptionD;
            question.CorrectAnswer = model.CorrectAnswer;

            _context.SaveChanges();

            return RedirectToAction("AddQuestion", new
            {
                id = question.AssessmentId
            });
        }


        // Delete Question
        public IActionResult DeleteQuestion(int id)
        {
            var question = _context.QuizQuestions
                .FirstOrDefault(q => q.QuestionId == id);

            if (question == null)
            {
                return NotFound();
            }

            int assessmentId = question.AssessmentId;

            _context.QuizQuestions.Remove(question);
            _context.SaveChanges();

            return RedirectToAction("AddQuestion", new
            {
                id = assessmentId
            });
        }
        public IActionResult PublishAssessment(int id)
        {
            var assessment = _context.Assessments
                .Include(a => a.QuizQuestions)
                .FirstOrDefault(a => a.AssessmentId == id);

            if (assessment == null)
            {
                return NotFound();
            }

            if (!assessment.QuizQuestions.Any())
            {
                TempData["Message"] = "Please add at least one question before publishing.";

                return RedirectToAction("AddQuestion", new { id });
            }

            assessment.IsPublished = true;

            _context.SaveChanges();

            TempData["Message"] = "Quiz published successfully.";

            return RedirectToAction("Index");
        }

        public IActionResult UnpublishAssessment(int id)
        {
            var assessment = _context.Assessments
                .FirstOrDefault(a => a.AssessmentId == id);

            if (assessment == null)
            {
                return NotFound();
            }

            assessment.IsPublished = false;

            _context.SaveChanges();

            TempData["Message"] = "Assessment unpublished successfully.";

            return RedirectToAction("ManageAssessment", new { id });
        }

    }
    

}