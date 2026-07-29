using SkyTech.Models;
using SkyTech.Models.JobPostServices;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using static SkyTech.Models.JobPostServices.By_TRENCH.Enums.StaffAnalysisEnums;

namespace SkyTech.JobPostServices
{
    public class OpenRouterJobPostDraftService : IAIJobPostDraftService
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;

        public OpenRouterJobPostDraftService(HttpClient http, IConfiguration config)
        {
            _http = http;
            _config = config;
        }

        public async Task<JobPostDraftResult> GenerateDraftFromRoleAsync(JobRoleKnowledge role)
        {
            var prompt = $@"
You are formatting an internal, company-approved job advert for SkyTech.

You must use ONLY the information given below. Do not add, assume, or invent
any responsibility, skill, experience requirement, qualification, salary detail,
or company policy that is not explicitly listed. If a section is empty, omit it
or write 'Not specified' — do not fill it in yourself.

Job Title: {role.JobTitle}

Approved Responsibilities:
{role.CompanyResponsibilities}

Approved Required Skills:
{role.RequiredSkills}

Approved Required Experience:
{role.RequiredExperience}

Approved Required Qualifications:
{role.RequiredQualifications}

Other Approved Company Information:
{role.OtherCompanyInformation}

Task: rewrite the above into clear, professional prose suitable for a job ad.
You may reorganize, fix grammar, and improve wording. You may NOT add new content.

Return ONLY valid JSON, no markdown fences:
{{
  ""responsibilities"": ""..."",
  ""requiredCerts"": ""..."",
  ""requiredSkillsAndXP"": ""...""
}}";

            var body = new
            {
                model = _config["OpenRouterSettings:Model"],
                messages = new[] { new { role = "user", content = prompt } },
                temperature = 0.2 // low temperature = less creative drift, stays closer to source text
            };

            var req = new HttpRequestMessage(HttpMethod.Post, _config["OpenRouterSettings:Endpoint"])
            {
                Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
            };
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _config["OpenRouterSettings:ApiKey"]);

            var response = await _http.SendAsync(req);
            var raw = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                throw new Exception($"OpenRouter API error {(int)response.StatusCode}: {raw}");

            using var doc = JsonDocument.Parse(raw);
            var content = doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();

            content = content!.Trim();
            if (content.StartsWith("```"))
            {
                content = content.Trim('`');
                if (content.StartsWith("json")) content = content.Substring(4);
                content = content.Trim();
            }

            return JsonSerializer.Deserialize<JobPostDraftResult>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        }

        public async Task<JobPostDraftResult> GenerateDraftAsync(JobPostDraftRequest request)
        {
            var prompt = $@"
You are writing a job advert for a retail store called SkyTech.
Job Title: {request.JobTitle}
Department: {request.Department}
Location: {request.Location}
Employment Type: {request.EmploymentType}
Salary Range: {request.SalaryMin} - {request.SalaryMax}

Return ONLY valid JSON with these exact keys, no extra text, no markdown fences:
{{
  ""responsibilities"": ""a clear paragraph of day-to-day duties"",
  ""requiredCerts"": ""relevant certifications, or 'None required' if not applicable"",
  ""requiredSkillsAndXP"": ""relevant skills and years of experience expected""
}}";

            var body = new
            {
                model = _config["OpenRouterSettings:Model"],
                messages = new[] { new { role = "user", content = prompt } }
            };

            var req = new HttpRequestMessage(HttpMethod.Post, _config["OpenRouterSettings:Endpoint"])
            {
                Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
            };
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _config["OpenRouterSettings:ApiKey"]);

            var response = await _http.SendAsync(req);
            var raw = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"OpenRouter API error {(int)response.StatusCode}: {raw}");

            using var doc = JsonDocument.Parse(raw);
            var content = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            content = content!.Trim();
            if (content.StartsWith("```"))
            {
                content = content.Trim('`');
                if (content.StartsWith("json"))
                    content = content.Substring(4);
                content = content.Trim();
            }

            var draft = JsonSerializer.Deserialize<JobPostDraftResult>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return draft!;
        }

        public async Task<JobPostDraftResult> GenerateDraftForVacancyAsync(JobVacancyDraftRequest request)
        {
            string vacancyContext = request.VacancyType == VacancyType.TemporaryLeaveCover
                ? $@"
This is a TEMPORARY/FIXED-TERM contract role.
Contract Duration: {request.ContractStartDate:yyyy-MM-dd} to {request.ContractEndDate:yyyy-MM-dd} (~{request.ContractDurationDays} days)
Background: This position is being filled to cover an employee on approved leave."
                : @"
This is a PERMANENT position.
Background: The department is understaffed and needs ongoing headcount.";

            var prompt = $@"
You are writing a professional job advert for SkyTech, a retail company.

{vacancyContext}

Job Title: {request.JobTitle}
Department: {request.Department}
Headcount Needed: {request.HeadcountNeeded} position(s)

Based on the job title and department, create a realistic job post that includes:
- Day-to-day responsibilities specific to this role in a retail environment
- Required skills and experience level
- Any certifications or qualifications that would be relevant

You must NOT add salary, location, benefits, or any company-specific policy details 
that are not universally known about SkyTech. Keep to the core role requirements.

Return ONLY valid JSON with these exact keys, no extra text, no markdown fences:
{{
  ""responsibilities"": ""a clear paragraph of day-to-day duties and key responsibilities"",
  ""requiredCerts"": ""relevant certifications or qualifications, or 'None required' if not applicable"",
  ""requiredSkillsAndXP"": ""relevant skills and years of experience expected""
}}";

            var body = new
            {
                model = _config["OpenRouterSettings:Model"],
                messages = new[] { new { role = "user", content = prompt } },
                temperature = 0.3
            };

            var req = new HttpRequestMessage(HttpMethod.Post, _config["OpenRouterSettings:Endpoint"])
            {
                Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
            };
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _config["OpenRouterSettings:ApiKey"]);

            var response = await _http.SendAsync(req);
            var raw = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"OpenRouter API error {(int)response.StatusCode}: {raw}");

            using var doc = JsonDocument.Parse(raw);
            var content = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            content = content!.Trim();
            if (content.StartsWith("```"))
            {
                content = content.Trim('`');
                if (content.StartsWith("json"))
                    content = content.Substring(4);
                content = content.Trim();
            }

            var draft = JsonSerializer.Deserialize<JobPostDraftResult>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return draft!;
        }
    }
}