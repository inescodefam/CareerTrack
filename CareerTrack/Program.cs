using CareerTrack.Factory;
using CareerTrack.Interfaces;
using CareerTrack.Repository;
using CareerTrack.Services;
using CareerTrack.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<ApiBehaviorOptions>(options =>
    options.SuppressModelStateInvalidFilter = true);


builder.Services.AddAuthentication()
  .AddCookie(options =>
  {
      options.LoginPath = "/User/Login";
      options.LogoutPath = "/User/Logout";
      options.AccessDeniedPath = "/User/Forbidden";
      options.SlidingExpiration = true;
      options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
  });


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
app.UseStaticFiles();

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
