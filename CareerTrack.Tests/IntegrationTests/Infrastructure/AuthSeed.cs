using CareerTrack.Models;
using CareerTrack.Security;
using Microsoft.Extensions.DependencyInjection;

namespace CareerTrack.Tests.IntegrationTests.Infrastructure
{
    public static class AuthSeed
    {
        public static void SeedUser(
            CustomWebApplicationFactory factory,
            string username,
            string password,
            string email = "seed@mail.com",
            bool isAdmin = false)
        {
            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var existing = db.Users.FirstOrDefault(u => u.UserName == username);
            if (existing != null)
            {
                db.Users.Remove(existing);
                db.SaveChanges();
            }

            var salt = PasswordHashProvider.GetSalt();
            var hash = PasswordHashProvider.GetHash(password, salt);

            db.Users.Add(new User
            {
                UserName = username,
                Email = email,
                FirstName = "Seed",
                LastName = "User",
                PasswordSalt = salt,
                PasswordHash = hash,
                IsAdmin = isAdmin
            });

            db.SaveChanges();
        }
    }
}
