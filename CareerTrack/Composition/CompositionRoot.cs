using CareerTrack.Data;
using CareerTrack.Security;
using CareerTrack.Services;
using Microsoft.AspNetCore.Identity;

namespace CareerTrack.Composition{
    public static class CompositionRoot
    {
        private static AuthService CreateAuth(AppDbContext db, IHttpContextAccessor http)
        {
            IUserRepository userRepo = new UserRepository(db);
            IRoleResolver roleResolver = new DefaultRoleResolver();
            IAuthCookieService cookieService = new AuthCookieService(http);

            return new AuthService(userRepo, cookieService, roleResolver);
        }

        public static ILoginService CreateLoginService(AppDbContext db, IHttpContextAccessor http)
            => CreateAuth(db, http);

        public static IRegistrationService CreateRegistrationService(AppDbContext db, IHttpContextAccessor http)
            => CreateAuth(db, http);

        public static ILogoutService CreateLogoutService(AppDbContext db, IHttpContextAccessor http)
            => CreateAuth(db, http);
    }
}
