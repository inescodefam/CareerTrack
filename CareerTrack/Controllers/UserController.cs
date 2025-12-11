using CareerTrack.Models;
using CareerTrack.Security;
using CareerTrack.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CareerTrack.Controllers
{
    public class UserController : Controller
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl)
        {
            var loginVm = new UserLoginVM
            {
                ReturnUrl = returnUrl
            };
            return View();
        }

        [HttpPost]
        public IActionResult Login(UserLoginVM sentUserToLogin)
        {

            string genericLoginError = "Incorrect username or password";

            var username = sentUserToLogin.Username.Trim();

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(sentUserToLogin.Password))
            {
                ModelState.AddModelError("", genericLoginError);
                return View(sentUserToLogin);
            }


            var existingUser = _context.Users
                .AsNoTracking()
                .FirstOrDefault(u => u.UserName.ToLower() == username.ToLower());


            if (existingUser == null)
            {
                ModelState.AddModelError("", genericLoginError);
                return View(sentUserToLogin);
            }


            var hashToCheck = PasswordHashProvider.GetHash(sentUserToLogin.Password, existingUser.PasswordSalt);
            if (hashToCheck != existingUser.PasswordHash)
            {
                ModelState.AddModelError("", genericLoginError);
                return View(sentUserToLogin);
            }

            string role = existingUser.IsAdmin ? "Admin" : "User";


            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, sentUserToLogin.Username),
                new Claim(ClaimTypes.Role, role)
            };

            var claimsIdentity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties();

            // We need to wrap async code here into synchronous since we don't use async methods
            Task.Run(async () =>
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties)
            ).GetAwaiter().GetResult();

            Console.WriteLine(existingUser.PasswordHash);
            Console.WriteLine(existingUser.PasswordSalt);


            if (sentUserToLogin.ReturnUrl != null)
                return LocalRedirect(sentUserToLogin.ReturnUrl);
            else if (existingUser.IsAdmin)
                return RedirectToAction("Index", "Home");
            else if (!existingUser.IsAdmin)
                return RedirectToAction("Index", "Home");
            else
                return View();
        }

        [Authorize]
        public IActionResult Logout()
        {
            Task.Run(async () =>
                await HttpContext.SignOutAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme)
            ).GetAwaiter().GetResult();

            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }


        [HttpPost]
        public IActionResult Register(UserRegisterVM sentUserToRegister)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }
            try
            {
                var username = sentUserToRegister.Username.Trim();
                var email = sentUserToRegister.Email.Trim();

                if (_context.Users.Any(u => u.UserName == username))
                {
                    ModelState.AddModelError("", $"Username {username} is already taken");
                    return View(sentUserToRegister);

                }

                if (_context.Users.Any(u => u.Email == email))
                {
                    ModelState.AddModelError("", $"Email {email} is already taken");
                    return View(sentUserToRegister);

                }

                var salt = PasswordHashProvider.GetSalt();
                var hash = PasswordHashProvider.GetHash(sentUserToRegister.Password, salt);

                User newUser = new User
                {
                    FirstName = sentUserToRegister.FirstName,
                    LastName = sentUserToRegister.LastName,
                    UserName = username,
                    Email = email,
                    PasswordHash = hash,
                    PasswordSalt = salt,
                    Phone = sentUserToRegister.Phone,
                };

                _context.Users.Add(newUser);
                _context.SaveChanges();

                return RedirectToAction("Login", "User");

            }
            catch (Exception)
            {
                ModelState.AddModelError("", "There has been a problem registering a user");
                return View(sentUserToRegister);
            }
        }
    }
}
