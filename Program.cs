using CM_Project.Models;
using CM_Project.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

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

// Your Services class (email + DI)
builder.Services.AddScoped<EmailServices>();

builder.Services.AddScoped<PayStackService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
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
    pattern: "{controller=Authentication}/{action=Home}/{id?}"); // ✅ starts at Login

app.Run();
