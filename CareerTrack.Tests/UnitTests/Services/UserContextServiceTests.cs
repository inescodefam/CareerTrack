using System;
using System.Security.Claims;
using System.Threading.Tasks;
using CareerTrack.Models;
using CareerTrack.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace CareerTrack.Tests.UnitTests.Services
{
    public class UserContextServiceTests
    {
        private static AppDbContext CreateInMemoryDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            return new AppDbContext(options);
        }

        private static IHttpContextAccessor CreateHttpContextAccessorWithUser(string? username)
        {
            var httpContext = new DefaultHttpContext();

            if (username != null)
            {
                var claimsIdentity = new ClaimsIdentity(
                    new[] { new Claim(ClaimTypes.Name, username) },
                    authenticationType: "TestAuth");

                httpContext.User = new ClaimsPrincipal(claimsIdentity);
            }
            else
            {
                httpContext.User = new ClaimsPrincipal(new ClaimsIdentity());
            }

            return new HttpContextAccessor { HttpContext = httpContext };
        }


        [Fact]
        public void GetCurrentUsername_ShouldReturnUsername_WhenAuthenticated()
        {
            // Arrange
            using var db = CreateInMemoryDbContext(nameof(GetCurrentUsername_ShouldReturnUsername_WhenAuthenticated));
            var accessor = CreateHttpContextAccessorWithUser("mmarkic");
            UserContextService userContextService = new UserContextService(accessor, db);

            // Act
            var username = userContextService.GetCurrentUsername();

            // Assert
            username.Should().Be("mmarkic");
        }

        [Fact]
        public void GetCurrentUsername_ShouldThrowUnauthorized_WhenNotAuthenticated()
        {
            //Arrange
            using var db = CreateInMemoryDbContext(nameof(GetCurrentUsername_ShouldThrowUnauthorized_WhenNotAuthenticated));

            var accessor = CreateHttpContextAccessorWithUser(username: null);

            var userContextService = new UserContextService(accessor, db);

            //Act
            Action act = () => userContextService.GetCurrentUsername();

            //Assert
            act.Should()
               .Throw<UnauthorizedAccessException>()
               .WithMessage("User not authenticated");
        }

        [Fact]
        public void GetCurrentUsername_ShouldThrowUnauthorized_WhenHttpContextIsNull()
        {
            //Arrange
            using var db = CreateInMemoryDbContext(nameof(GetCurrentUsername_ShouldThrowUnauthorized_WhenHttpContextIsNull));
            var accessorMock = new Mock<IHttpContextAccessor>();
            accessorMock.Setup(a => a.HttpContext).Returns((HttpContext?)null);

            var userContextService = new UserContextService(accessorMock.Object, db);

            //Act
            Action act = () => userContextService.GetCurrentUsername();

            //Assert
            act.Should()
               .Throw<UnauthorizedAccessException>()
               .WithMessage("User not authenticated");
        }

        [Fact]
        public void GetCurrentUserId_ShouldReturnUserId_WhenUserExistsInDatabase()
        {
            //Arrange
            using var db = CreateInMemoryDbContext(nameof(GetCurrentUserId_ShouldReturnUserId_WhenUserExistsInDatabase));

            db.Users.Add(new User
            {
                Id = 42,
                UserName = "mmarkic",
                Email = "mmarkic@gmail.com",
                FirstName = "M",
                LastName = "Markic",
                PasswordHash = "x",
                PasswordSalt = "y",
                IsAdmin = false
            });
            db.SaveChanges();

            var accessor = CreateHttpContextAccessorWithUser("mmarkic");
            var userContextService = new UserContextService(accessor, db);

            //Act
            var userId = userContextService.GetCurrentUserId();

            //Assert
            userId.Should().Be(42);
        }

        [Fact]
        public void GetCurrentUserId_ShouldThrowUnauthorized_WhenUserNotFoundInDatabase()
        {
            //Arrange
            using var db = CreateInMemoryDbContext(nameof(GetCurrentUserId_ShouldThrowUnauthorized_WhenUserNotFoundInDatabase));
            var accessor = CreateHttpContextAccessorWithUser("doesNotExist");
            var userContextService = new UserContextService(accessor, db);

            //Act
            Action act = () => userContextService.GetCurrentUserId();

            //Assert
            act.Should()
               .Throw<UnauthorizedAccessException>()
               .WithMessage("User not found");
        }

        [Fact]
        public void GetCurrentUserId_ShouldThrowUnauthorized_WhenNotAuthenticated()
        {
            //Arrange
            using var db = CreateInMemoryDbContext(nameof(GetCurrentUserId_ShouldThrowUnauthorized_WhenNotAuthenticated));
            var accessor = CreateHttpContextAccessorWithUser(username: null);
            var userContextService = new UserContextService(accessor, db);

            //Act
            Action act = () => userContextService.GetCurrentUserId();

            //Assert
            act.Should()
               .Throw<UnauthorizedAccessException>()
               .WithMessage("User not authenticated");
        }
    }
}
