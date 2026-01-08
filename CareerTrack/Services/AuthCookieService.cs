using CareerTrack.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace CareerTrack.Services
{
    public class AuthCookieService : IAuthCookieService
    {

        private readonly IHttpContextAccessor _http;

        public AuthCookieService(IHttpContextAccessor http)
        {
            _http = http;
        }

        private HttpContext HttpContext =>
            _http.HttpContext ?? throw new InvalidOperationException("No active HttpContext.");

        public Task SignInAsync(User user, string role)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Role, role),
            };

            var claimsIdentity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties();

            return HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);
        }

        public Task SignOutAsync()
        {
            return HttpContext.SignOutAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }
}
