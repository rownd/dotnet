using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Rownd;
using Rownd.Models;
using RowndASPTestApp.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options => {
    options.UseSqlite(connectionString);
    options.EnableSensitiveDataLogging();
});
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddRazorPages();

builder.Services.AddControllersWithViews();

builder.Services.AddSingleton<Config>(sp => {
    return new Config(builder.Configuration["Rownd:AppKey"], builder.Configuration["Rownd:AppSecret"]);
});
builder.Services.AddSingleton<RowndClient>();

//builder.Services.AddAuthentication(options => options.DefaultScheme = "rownd_auth")
  //  .AddScheme<RowndAuthOptions, RowndAuthHandler>("rownd_auth", options => { });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

using (var scope = app.Services.CreateScope())
{

    ApplicationDbContext _db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    UserManager<IdentityUser> _userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    RoleManager<IdentityRole> _roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    // Seed Roles
    List<IdentityRole> roles = new()
    {
        new IdentityRole("admin"),
        new IdentityRole("client")
    };

    foreach (IdentityRole role in roles)
    {
        if (!_db.Roles.Contains(role))
        {
            await _roleManager.CreateAsync(role);
        }
    }
}

app.Run();
