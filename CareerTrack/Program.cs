using CareerTrack.Data;
using CareerTrack.Factory;
using CareerTrack.Interfaces;
using CareerTrack.Middlware.CareerTrack.Middleware;
using CareerTrack.Repository;
using CareerTrack.Security;
using CareerTrack.Services;
using CareerTrack.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

//ensure ModelState check
builder.Services.Configure<ApiBehaviorOptions>(options =>
    options.SuppressModelStateInvalidFilter = true);


builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthCookieService, AuthCookieService>();
//builder.Services.AddScoped<IAuthService, AuthService>();

//builder.Services.AddScoped<ILoginService, ILoginService>();
//builder.Services.AddScoped<IRegistrationService, IRegistrationService>();
//builder.Services.AddScoped<ILogoutService, ILogoutService>();


builder.Services.AddScoped<IRoleResolver, DefaultRoleResolver>();

//OCP demonstracija
//builder.Services.AddScoped<IRoleResolver, PremiumRoleResolver>();

//builder.Services.AddScoped<IRoleResolver, BadRoleResolver>();


//cookie auth
builder.Services.AddAuthentication()
  .AddCookie(options =>
  {
      options.LoginPath = "/User/Login";
      options.LogoutPath = "/User/Logout";
      options.AccessDeniedPath = "/User/Forbidden";
      options.SlidingExpiration = true;
      options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
  });

// http context
builder.Services.AddHttpContextAccessor();

// register:
// Services
builder.Services.AddScoped<IGoalService, GoalService>();
builder.Services.AddScoped<IProgressService, ProgressService>();
builder.Services.AddScoped<IUserContextService, UserContextService>();
builder.Services.AddScoped<IGoalExportService, GoalExportService>();
builder.Services.AddScoped<IGoalFactory, GoalFactory>();

// Repositories
builder.Services.AddScoped<IGoalRepository, GoalRepository>();

// Utilities
builder.Services.AddScoped<IDateTimeConverter, DateTimeConverter>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseHttpsRedirection();
// with extension method - oæu da izgleda ko ostale :)
app.UseSecurityHeaders();
app.UseStaticFiles();
// register middleware
//app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated();
}

app.Run();


public partial class Program { }
