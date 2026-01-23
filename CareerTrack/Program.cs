using CareerTrack.Data;
using CareerTrack.Factory;
using CareerTrack.Interfaces;
using CareerTrack.Middlware.CareerTrack.Middleware;
using CareerTrack.Models;
using CareerTrack.Repository;
using CareerTrack.Security;
using CareerTrack.Services;
using CareerTrack.Utilities;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddControllersWithViews();

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<ApiBehaviorOptions>(options =>
    options.SuppressModelStateInvalidFilter = true);


builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthCookieService, AuthCookieService>();

builder.Services.AddScoped<IRoleResolver, DefaultRoleResolver>();


var isProduction = builder.Environment.IsProduction();
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        // Paths
        options.LoginPath = "/User/Login";
        options.LogoutPath = "/User/Logout";
        options.AccessDeniedPath = "/User/Forbidden";

        // Lifetime
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(20);

        // Cookie security
        options.Cookie.Name = ".CareerTrack.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = isProduction
            ? CookieSecurePolicy.None  // Render handles HTTPS
            : CookieSecurePolicy.Always; // <-- Secure flag always
        options.Cookie.SameSite = SameSiteMode.Strict;           // <-- CSRF hardening (Strict or Lax)
        options.Cookie.IsEssential = true;                       // avoids being blocked by consent features
    });

builder.Services.AddAntiforgery(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = isProduction
        ? CookieSecurePolicy.None
        : CookieSecurePolicy.Always; // <-- Secure flag
    options.Cookie.SameSite = SameSiteMode.Strict;           // or Lax
});


builder.Services.AddHsts(options =>
{
    options.Preload = true;
    options.IncludeSubDomains = true;
    options.MaxAge = TimeSpan.FromDays(365);
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IGoalService, GoalService>();
builder.Services.AddScoped<IProgressService, ProgressService>();
builder.Services.AddScoped<IUserContextService, UserContextService>();
builder.Services.AddScoped<IGoalExportService, GoalExportService>();
builder.Services.AddScoped<IGoalFactory, GoalFactory>();

builder.Services.AddScoped<IGoalRepository, GoalRepository>();

builder.Services.AddScoped<IDateTimeConverter, DateTimeConverter>();


var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseForwardedHeaders();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseSecurityHeaders();
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
    await context.Database.EnsureCreatedAsync();
}
await app.RunAsync();

public partial class Program
{
    protected Program() { }
}