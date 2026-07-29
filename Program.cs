
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.EntityFrameworkCore;
using SkyTech.JobPostServices;
using SkyTech.Models;
using SkyTech.Models.JobPostServices;
using SkyTech.Models.JobPostServices.By_TRENCH;
using SkyTech.Models.Repair_Item_Services;
using SkyTech.Services;
using System.Globalization;
using static SkyTech.Models.JobPostServices.By_TRENCH.Enums.StaffAnalysisEnums;

var builder = WebApplication.CreateBuilder(args);

// Force dot as decimal separator for model binding
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
// MVC
builder.Services.AddControllersWithViews();

// SQL
//this is a way of pulling the connection string in json file
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");  //this "DefaultConnection" must match with the one in json file connection string

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));


// Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();
builder.Services.AddScoped<PayStackTransferService>();


// I moved this from the bottom to up heereeeeeeeeeeeeeee
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});


//Hangfire configuration
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(connectionString));

builder.Services.AddHangfireServer();

// Your Services class (email + DI)
builder.Services.AddScoped<EmailServices>();
builder.Services.AddHttpClient<IAIJobPostDraftService, OpenRouterJobPostDraftService>();
builder.Services.AddHttpClient<ICvExtractionService, OpenRouterCvExtractionService>();
//builder.Services.AddHttpClient<IAIJobPostDraftService, GeminiJobPostDraftService>();
builder.Services.AddScoped<PayStackService>();
builder.Services.AddScoped<IStaffingAnalysisService, StaffingAnalysisService>();

var app = builder.Build();


// Okay Team This here will repopulate the dropdown list for Technicians specializations and on repair requsts
using ( var scope = app.Services.CreateScope())
{
    var DatabaseContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    if (!DatabaseContext.RepairItemCategories.Any())
    {
        DatabaseContext.RepairItemCategories.AddRange(
                new Repair_Item_Category { CategoryName = "Phones" },
                new Repair_Item_Category { CategoryName = "Laptops" },
                new Repair_Item_Category { CategoryName = "Consoles" }
            );

        DatabaseContext.SaveChanges();
    }
}


// Enable Hangfire Dashboard (optional, for monitoring)
app.UseHangfireDashboard("/hangfire");

// Schedule recurring analysis jobs
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var analysisService = scope.ServiceProvider.GetRequiredService<IStaffingAnalysisService>();

    // Get all active schedules and register them
    var activeSchedules = db.StaffingAnalysisSchedules.Where(s => s.IsActive).ToList();
    foreach (var schedule in activeSchedules)
    {
        RegisterScheduledAnalysis(schedule, analysisService);
    }
}


void RegisterScheduledAnalysis(StaffingAnalysisSchedule schedule, IStaffingAnalysisService analysisService)
{
    string jobId = $"staffing-analysis-{schedule.Id}";

    switch (schedule.Frequency)
    {
        case AnalysisFrequency.Daily:
            string cronDaily = $"{schedule.TimeOfDay.Minutes} {schedule.TimeOfDay.Hours} * * *";
            RecurringJob.AddOrUpdate(
                jobId,
                () => analysisService.RunAnalysisAsync(schedule.Id, null),
                cronDaily);
            break;

        case AnalysisFrequency.Weekly:
            string cronWeekly = $"{schedule.TimeOfDay.Minutes} {schedule.TimeOfDay.Hours} ? * MON";
            RecurringJob.AddOrUpdate(
                jobId,
                () => analysisService.RunAnalysisAsync(schedule.Id, null),
                cronWeekly);
            break;

        case AnalysisFrequency.Monthly:
            string cronMonthly = $"{schedule.TimeOfDay.Minutes} {schedule.TimeOfDay.Hours} 1 * *";
            RecurringJob.AddOrUpdate(
                jobId,
                () => analysisService.RunAnalysisAsync(schedule.Id, null),
                cronMonthly);
            break;


        //case 0: // Daily
        //    RecurringJob.AddOrUpdate( jobId, () => analysisService.RunAnalysisAsync(schedule.Id, null),Cron.Daily(schedule.TimeOfDay.Hours, schedule.TimeOfDay.Minutes));
        //    break;
        //case 1: // Weekly
        //    RecurringJob.AddOrUpdate(
        //        jobId,
        //        () => analysisService.RunAnalysisAsync(schedule.Id, null),
        //        Cron.Weekly(DayOfWeek.Monday, schedule.TimeOfDay.Hours, schedule.TimeOfDay.Minutes));
        //    break;
        //case 2: // Monthly
        //    RecurringJob.AddOrUpdate(
        //        jobId,
        //        () => analysisService.RunAnalysisAsync(schedule.Id, null),
        //        Cron.Monthly(1, schedule.TimeOfDay.Hours, schedule.TimeOfDay.Minutes));
        //    break;
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/ShoppingPage/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();       // ✅ serves wwwroot (CSS/JS)
app.UseRouting();
app.UseSession();           // ✅ must be BEFORE MapControllerRoute
app.UseAuthorization();



//route config for core
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Customer}/{action=ShoppingPage}/{id?}"); // starts on the shopping page

app.Run();

