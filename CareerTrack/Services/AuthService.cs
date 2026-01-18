using CareerTrack.Data;
using CareerTrack.Models;
using CareerTrack.Security;
using CareerTrack.ViewModels;

namespace CareerTrack.Services
{
    public class AuthService : ILoginService, IRegistrationService, ILogoutService
    {

        private readonly IUserRepository _users;
        private readonly IAuthCookieService _cookie;
        private readonly IRoleResolver _roleResolver;

        public AuthService(IUserRepository users, IAuthCookieService cookie, IRoleResolver roleResolver)
        {
            _users = users;
            _cookie = cookie;
            _roleResolver = roleResolver;
        }

        public async Task<AuthResult> LoginAsync(UserLoginVM userLoginVM)
        {
            const string genericLoginError = "Incorrect username or password";

            var username = (userLoginVM.Username ?? "").Trim();
            var password = userLoginVM.Password ?? "";


            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return new(false, genericLoginError, null);


            var user = await _users.FindByUsernameAsync(username);
            if (user == null)
            {
                return new(false, genericLoginError, null);
            }


            var hashToCheck = PasswordHashProvider.GetHash(password, user.PasswordSalt);
            if (hashToCheck != user.PasswordHash)
                return new(false, genericLoginError, null);


            string role = _roleResolver.ResolveRole(user);
            await _cookie.SignInAsync(user, role);

            return new(true, null, userLoginVM.ReturnUrl ?? "/");
        }

        public async Task LogoutAsync()
        {
            await _cookie.SignOutAsync();
        }

        public async Task<AuthResult> RegisterAsync(UserRegisterVM userRegisterVM)
        {
            var username = (userRegisterVM.Username ?? "").Trim();
            var email = (userRegisterVM.Email ?? "").Trim();

            if (await _users.ExistsByUsernameAsync(username))
                return new(false, $"Username {username} is already taken", null);

            if (await _users.ExistsByEmailAsync(email))
                return new(false, $"Email {email} is already taken", null);


            var salt = PasswordHashProvider.GetSalt();
            var hash = PasswordHashProvider.GetHash(userRegisterVM.Password, salt);

            var newUser = new User
            {
                FirstName = userRegisterVM.FirstName,
                LastName = userRegisterVM.LastName,
                UserName = username,
                Email = email,
                PasswordSalt = salt,
                PasswordHash = hash,
                Phone = string.IsNullOrWhiteSpace(userRegisterVM.Phone) ? null : userRegisterVM.Phone.Trim(),
                IsAdmin = false
            };

            await _users.AddAsync(newUser);
            await _users.SaveChangesAsync();

            return new(true, null, "/User/Login");
        }
    }
}
