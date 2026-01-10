using CareerTrack.Models;
using CareerTrack.Security;
using CareerTrack.Services;
using CareerTrack.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Plugins;
using System.Security.Claims;

namespace CareerTrack.Controllers
{
    public class UserController : Controller
    {

        //private readonly IAuthService _authService;

        //public UserController(IAuthService authService)
        //{
        //    _authService = authService;
        //}

        private readonly ILoginService _loginService;
        private readonly IRegistrationService _registrationService;
        private readonly ILogoutService _logoutService;


        public UserController(AppDbContext db, IHttpContextAccessor http)
        {
            _loginService = CompositionRoot.CreateLoginService(db, http);
            _registrationService = CompositionRoot.CreateRegistrationService(db, http);
            _logoutService = CompositionRoot.CreateLogoutService(db, http);
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
        public async Task<IActionResult> Login(UserLoginVM vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var result = await _loginService.LoginAsync(vm);
            if (!result.success)
            {
                ModelState.AddModelError("", result.errorMessage ?? "Login failed");
                return View(vm);
            }

            if (!string.IsNullOrWhiteSpace(result.reirectUrl) && Url.IsLocalUrl(result.reirectUrl))
                return LocalRedirect(result.reirectUrl);

            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _logoutService.LogoutAsync();
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Register(UserRegisterVM vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var result = await _registrationService.RegisterAsync(vm);
            if (!result.success)
            {
                ModelState.AddModelError("", result.errorMessage ?? "There has been a problem registering a user");
                return View(vm);
            }

            return Redirect(result.reirectUrl ?? "/User/Login");
        }
    }
}
