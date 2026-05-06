using CM_Project.Models;
using CM_Project.Services;
using Microsoft.EntityFrameworkCore;
using SkyTech.Models.Repair_Item_Services;
using System.Globalization;

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


// I moved this from the bottom to up heereeeeeeeeeeeeeee
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});


// Your Services class (email + DI)
builder.Services.AddScoped<EmailServices>();

builder.Services.AddScoped<PayStackService>();

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
    pattern: "{controller=Authentication}/{action=Home}/{id?}"); // starts on the shopping page

app.Run();

