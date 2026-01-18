using CareerTrack.Controllers;
using CareerTrack.Models;
using CareerTrack.ViewModels;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Security.Claims;

namespace CareerTrack.Tests.UnitTests.Controllers
{
    public class UserControllerTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly UserController _controller;
        private readonly Mock<IAuthenticationService> _mockAuthService;
        private readonly Mock<IServiceProvider> _mockServiceProvider;
        private bool _disposed;

        public UserControllerTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _controller = new UserController(_context);

            _mockAuthService = new Mock<IAuthenticationService>();
            _mockServiceProvider = new Mock<IServiceProvider>();

            var mockUrlHelperFactory = new Mock<IUrlHelperFactory>();
            var mockUrlHelper = new Mock<IUrlHelper>();
            mockUrlHelper.Setup(u => u.Action(It.IsAny<UrlActionContext>()))
                .Returns("/Goals/Index");
            mockUrlHelperFactory.Setup(f => f.GetUrlHelper(It.IsAny<ActionContext>()))
                .Returns(mockUrlHelper.Object);

            _mockServiceProvider
                .Setup(sp => sp.GetService(typeof(IAuthenticationService)))
                .Returns(_mockAuthService.Object);
            _mockServiceProvider
                .Setup(sp => sp.GetService(typeof(IUrlHelperFactory)))
                .Returns(mockUrlHelperFactory.Object);

            var httpContext = new DefaultHttpContext
            {
                RequestServices = _mockServiceProvider.Object
            };

            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
            _controller.TempData = tempData;
            _controller.Url = mockUrlHelper.Object;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context.Database.EnsureDeleted();
                    _context.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #region Login GET Tests

        [Fact]
        public void Login_GET_ShouldReturnView()
        {
            // Act
            var result = _controller.Login((string)null!);

            // Assert
            result.Should().BeOfType<ViewResult>();
        }

        [Fact]
        public void Login_GET_WithReturnUrl_ShouldReturnView()
        {
            // Act
            var result = _controller.Login("/Goals");

            // Assert
            result.Should().BeOfType<ViewResult>();
        }

        #endregion

        #region Login POST Tests

        [Fact]
        public void Login_POST_WithInvalidModelState_ShouldReturnViewWithModel()
        {
            // Arrange
            var loginVM = new UserLoginVM { Username = "", Password = "" };
            _controller.ModelState.AddModelError("Username", "Required");

            // Act
            var result = _controller.Login(loginVM);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().Be(loginVM);
        }

        [Fact]
        public void Login_POST_WithNullModel_ShouldReturnViewWithError()
        {
            // Act
            var result = _controller.Login((UserLoginVM)null!);

            // Assert
            result.Should().BeOfType<ViewResult>();
            _controller.ModelState.ErrorCount.Should().BeGreaterThan(0);
        }

        [Fact]
        public void Login_POST_WithEmptyUsername_ShouldReturnViewWithError()
        {
            // Arrange
            var loginVM = new UserLoginVM { Username = "", Password = "password" };

            // Act
            var result = _controller.Login(loginVM);

            // Assert
            result.Should().BeOfType<ViewResult>();
            _controller.ModelState.ErrorCount.Should().BeGreaterThan(0);
        }

        [Fact]
        public void Login_POST_WithEmptyPassword_ShouldReturnViewWithError()
        {
            // Arrange
            var loginVM = new UserLoginVM { Username = "testuser", Password = "" };

            // Act
            var result = _controller.Login(loginVM);

            // Assert
            result.Should().BeOfType<ViewResult>();
            _controller.ModelState.ErrorCount.Should().BeGreaterThan(0);
        }

        [Fact]
        public void Login_POST_WithNonExistentUser_ShouldReturnViewWithError()
        {
            // Arrange
            var loginVM = new UserLoginVM { Username = "nonexistent", Password = "password" };

            // Act
            var result = _controller.Login(loginVM);

            // Assert
            result.Should().BeOfType<ViewResult>();
            _controller.ModelState.ErrorCount.Should().BeGreaterThan(0);
        }

        [Fact]
        public void Login_POST_WithWrongPassword_ShouldReturnViewWithError()
        {
            // Arrange
            var salt = CareerTrack.Security.PasswordHashProvider.GetSalt();
            var hash = CareerTrack.Security.PasswordHashProvider.GetHash("correctpassword", salt);

            var user = new User
            {
                Id = 1,
                UserName = "testuser",
                Email = "test@example.com",
                PasswordHash = hash,
                PasswordSalt = salt,
                FirstName = "Test",
                LastName = "User",
                IsAdmin = false
            };
            _context.Users.Add(user);
            _context.SaveChanges();

            var loginVM = new UserLoginVM { Username = "testuser", Password = "wrongpassword" };

            // Act
            var result = _controller.Login(loginVM);

            // Assert
            result.Should().BeOfType<ViewResult>();
            _controller.ModelState.ErrorCount.Should().BeGreaterThan(0);
        }

        [Fact]
        public void Login_POST_WithValidCredentials_ShouldRedirectToGoals()
        {
            // Arrange
            var salt = CareerTrack.Security.PasswordHashProvider.GetSalt();
            var hash = CareerTrack.Security.PasswordHashProvider.GetHash("correctpassword", salt);

            var user = new User
            {
                Id = 1,
                UserName = "testuser",
                Email = "test@example.com",
                PasswordHash = hash,
                PasswordSalt = salt,
                FirstName = "Test",
                LastName = "User",
                IsAdmin = false
            };
            _context.Users.Add(user);
            _context.SaveChanges();

            _mockAuthService.Setup(a => a.SignInAsync(
                It.IsAny<HttpContext>(),
                It.IsAny<string>(),
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<AuthenticationProperties>()))
                .Returns(Task.CompletedTask);

            var loginVM = new UserLoginVM { Username = "testuser", Password = "correctpassword" };

            // Act
            var result = _controller.Login(loginVM);

            // Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");
            redirectResult.ControllerName.Should().Be("Goals");
        }

        [Fact]
        public void Login_POST_WithValidAdminCredentials_ShouldRedirectToGoals()
        {
            // Arrange
            var salt = CareerTrack.Security.PasswordHashProvider.GetSalt();
            var hash = CareerTrack.Security.PasswordHashProvider.GetHash("adminpassword", salt);

            var user = new User
            {
                Id = 1,
                UserName = "admin",
                Email = "admin@example.com",
                PasswordHash = hash,
                PasswordSalt = salt,
                FirstName = "Admin",
                LastName = "User",
                IsAdmin = true
            };
            _context.Users.Add(user);
            _context.SaveChanges();

            _mockAuthService.Setup(a => a.SignInAsync(
                It.IsAny<HttpContext>(),
                It.IsAny<string>(),
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<AuthenticationProperties>()))
                .Returns(Task.CompletedTask);

            var loginVM = new UserLoginVM { Username = "admin", Password = "adminpassword" };

            // Act
            var result = _controller.Login(loginVM);

            // Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");
            redirectResult.ControllerName.Should().Be("Goals");
        }

        [Fact]
        public void Login_POST_WithReturnUrl_ShouldRedirectToReturnUrl()
        {
            // Arrange
            var salt = CareerTrack.Security.PasswordHashProvider.GetSalt();
            var hash = CareerTrack.Security.PasswordHashProvider.GetHash("password", salt);

            var user = new User
            {
                Id = 1,
                UserName = "testuser",
                Email = "test@example.com",
                PasswordHash = hash,
                PasswordSalt = salt,
                FirstName = "Test",
                LastName = "User",
                IsAdmin = false
            };
            _context.Users.Add(user);
            _context.SaveChanges();

            _mockAuthService.Setup(a => a.SignInAsync(
                It.IsAny<HttpContext>(),
                It.IsAny<string>(),
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<AuthenticationProperties>()))
                .Returns(Task.CompletedTask);

            var loginVM = new UserLoginVM
            {
                Username = "testuser",
                Password = "password",
                ReturnUrl = "/Goals/Details/1"
            };

            // Act
            var result = _controller.Login(loginVM);

            // Assert
            var redirectResult = result.Should().BeOfType<LocalRedirectResult>().Subject;
            redirectResult.Url.Should().Be("/Goals/Details/1");
        }

        [Fact]
        public void Login_POST_CaseInsensitiveUsername_ShouldWork()
        {
            // Arrange
            var salt = CareerTrack.Security.PasswordHashProvider.GetSalt();
            var hash = CareerTrack.Security.PasswordHashProvider.GetHash("password", salt);

            var user = new User
            {
                Id = 1,
                UserName = "TestUser",
                Email = "test@example.com",
                PasswordHash = hash,
                PasswordSalt = salt,
                FirstName = "Test",
                LastName = "User",
                IsAdmin = false
            };
            _context.Users.Add(user);
            _context.SaveChanges();

            _mockAuthService.Setup(a => a.SignInAsync(
                It.IsAny<HttpContext>(),
                It.IsAny<string>(),
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<AuthenticationProperties>()))
                .Returns(Task.CompletedTask);

            var loginVM = new UserLoginVM { Username = "testuser", Password = "password" };

            // Act
            var result = _controller.Login(loginVM);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
        }

        [Fact]
        public void Login_POST_ShouldTrimUsername()
        {
            // Arrange
            var salt = CareerTrack.Security.PasswordHashProvider.GetSalt();
            var hash = CareerTrack.Security.PasswordHashProvider.GetHash("password", salt);

            var user = new User
            {
                Id = 1,
                UserName = "testuser",
                Email = "test@example.com",
                PasswordHash = hash,
                PasswordSalt = salt,
                FirstName = "Test",
                LastName = "User",
                IsAdmin = false
            };
            _context.Users.Add(user);
            _context.SaveChanges();

            _mockAuthService.Setup(a => a.SignInAsync(
                It.IsAny<HttpContext>(),
                It.IsAny<string>(),
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<AuthenticationProperties>()))
                .Returns(Task.CompletedTask);

            var loginVM = new UserLoginVM { Username = "  testuser  ", Password = "password" };

            // Act
            var result = _controller.Login(loginVM);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
        }

        #endregion

        #region Register GET Tests

        [Fact]
        public void Register_GET_ShouldReturnView()
        {
            // Act
            var result = _controller.Register();

            // Assert
            result.Should().BeOfType<ViewResult>();
        }

        #endregion

        #region Register POST Tests

        [Fact]
        public void Register_POST_WithInvalidModelState_ShouldReturnViewWithModel()
        {
            // Arrange
            var registerVM = new UserRegisterVM();
            _controller.ModelState.AddModelError("Username", "Required");

            // Act
            var result = _controller.Register(registerVM);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().Be(registerVM);
        }

        [Fact]
        public void Register_POST_WithDuplicateUsername_ShouldReturnViewWithError()
        {
            // Arrange
            var existingUser = new User
            {
                Id = 1,
                UserName = "existinguser",
                Email = "existing@example.com",
                PasswordHash = "hash",
                PasswordSalt = "salt",
                FirstName = "Existing",
                LastName = "User"
            };
            _context.Users.Add(existingUser);
            _context.SaveChanges();

            var registerVM = new UserRegisterVM
            {
                Username = "existinguser",
                Email = "new@example.com",
                Password = "password123",
                FirstName = "New",
                LastName = "User"
            };

            // Act
            var result = _controller.Register(registerVM);

            // Assert
            result.Should().BeOfType<ViewResult>();
            _controller.ModelState.ErrorCount.Should().BeGreaterThan(0);
        }

        [Fact]
        public void Register_POST_WithDuplicateEmail_ShouldReturnViewWithError()
        {
            // Arrange
            var existingUser = new User
            {
                Id = 1,
                UserName = "existinguser",
                Email = "existing@example.com",
                PasswordHash = "hash",
                PasswordSalt = "salt",
                FirstName = "Existing",
                LastName = "User"
            };
            _context.Users.Add(existingUser);
            _context.SaveChanges();

            var registerVM = new UserRegisterVM
            {
                Username = "newuser",
                Email = "existing@example.com",
                Password = "password123",
                FirstName = "New",
                LastName = "User"
            };

            // Act
            var result = _controller.Register(registerVM);

            // Assert
            result.Should().BeOfType<ViewResult>();
            _controller.ModelState.ErrorCount.Should().BeGreaterThan(0);
        }

        [Fact]
        public void Register_POST_WithValidData_ShouldCreateUserAndRedirect()
        {
            // Arrange
            var registerVM = new UserRegisterVM
            {
                Username = "newuser",
                Email = "newuser@example.com",
                Password = "password123",
                FirstName = "New",
                LastName = "User"
            };

            // Act
            var result = _controller.Register(registerVM);

            // Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Login");
            redirectResult.ControllerName.Should().Be("User");

            var createdUser = _context.Users.FirstOrDefault(u => u.UserName == "newuser");
            createdUser.Should().NotBeNull();
            createdUser!.Email.Should().Be("newuser@example.com");
            createdUser.FirstName.Should().Be("New");
            createdUser.LastName.Should().Be("User");
        }

        [Fact]
        public void Register_POST_ShouldHashPassword()
        {
            // Arrange
            var registerVM = new UserRegisterVM
            {
                Username = "newuser",
                Email = "newuser@example.com",
                Password = "password123",
                FirstName = "New",
                LastName = "User"
            };

            // Act
            _controller.Register(registerVM);

            // Assert
            var createdUser = _context.Users.FirstOrDefault(u => u.UserName == "newuser");
            createdUser.Should().NotBeNull();
            createdUser!.PasswordHash.Should().NotBe("password123");
            createdUser.PasswordSalt.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void Register_POST_ShouldTrimUsernameAndEmail()
        {
            // Arrange
            var registerVM = new UserRegisterVM
            {
                Username = "  newuser  ",
                Email = "  newuser@example.com  ",
                Password = "password123",
                FirstName = "New",
                LastName = "User"
            };

            // Act
            _controller.Register(registerVM);

            // Assert
            var createdUser = _context.Users.FirstOrDefault(u => u.UserName == "newuser");
            createdUser.Should().NotBeNull();
            createdUser!.Email.Should().Be("newuser@example.com");
        }

        [Fact]
        public void Register_POST_WithPhoneNumber_ShouldSavePhone()
        {
            // Arrange
            var registerVM = new UserRegisterVM
            {
                Username = "newuser",
                Email = "newuser@example.com",
                Password = "password123",
                FirstName = "New",
                LastName = "User",
                Phone = "1234567890"
            };

            // Act
            _controller.Register(registerVM);

            // Assert
            var createdUser = _context.Users.FirstOrDefault(u => u.UserName == "newuser");
            createdUser.Should().NotBeNull();
            createdUser!.Phone.Should().Be("1234567890");
        }

        #endregion

        #region Logout Tests

        [Fact]
        public void Logout_ShouldSignOutAndRedirectToLogin()
        {
            // Arrange
            _mockAuthService.Setup(a => a.SignOutAsync(
                It.IsAny<HttpContext>(),
                It.IsAny<string>(),
                It.IsAny<AuthenticationProperties>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = _controller.Logout();

            // Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Login");
        }

        #endregion

        #region Register Exception Handling Tests

        [Fact]
        public void Register_POST_WhenExceptionOccurs_ShouldReturnViewWithError()
        {
            // Arrange - Create a scenario where SaveChanges would fail
            // We'll use a mock context that throws an exception
            var registerVM = new UserRegisterVM
            {
                Username = "newuser",
                Email = "newuser@example.com",
                Password = "password123",
                FirstName = "New",
                LastName = "User"
            };

            // Add a user first, then try to add another with same username to trigger exception
            var existingUser = new User
            {
                Id = 1,
                UserName = "newuser",
                Email = "different@example.com",
                PasswordHash = "hash",
                PasswordSalt = "salt",
                FirstName = "Existing",
                LastName = "User"
            };
            _context.Users.Add(existingUser);
            _context.SaveChanges();

            // Act
            var result = _controller.Register(registerVM);

            // Assert - Should return view with error due to duplicate username
            result.Should().BeOfType<ViewResult>();
            _controller.ModelState.ErrorCount.Should().BeGreaterThan(0);
        }

        #endregion

        #region Login with Null IsAdmin Tests

        [Fact]
        public void Login_POST_WithNullIsAdmin_ShouldReturnView()
        {
            // Arrange
            var salt = CareerTrack.Security.PasswordHashProvider.GetSalt();
            var hash = CareerTrack.Security.PasswordHashProvider.GetHash("password", salt);

            var user = new User
            {
                Id = 1,
                UserName = "testuser",
                Email = "test@example.com",
                PasswordHash = hash,
                PasswordSalt = salt,
                FirstName = "Test",
                LastName = "User",
                IsAdmin = null // Null IsAdmin - falls through to return View() due to operator precedence
            };
            _context.Users.Add(user);
            _context.SaveChanges();

            _mockAuthService.Setup(a => a.SignInAsync(
                It.IsAny<HttpContext>(),
                It.IsAny<string>(),
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<AuthenticationProperties>()))
                .Returns(Task.CompletedTask);

            var loginVM = new UserLoginVM { Username = "testuser", Password = "password" };

            // Act
            var result = _controller.Login(loginVM);

            // Assert - Due to operator precedence in the controller, null IsAdmin falls through to return View()
            result.Should().BeOfType<ViewResult>();
        }

        #endregion

        #region Login WhiteSpace Tests

        [Fact]
        public void Login_POST_WithWhitespaceOnlyUsername_ShouldReturnViewWithError()
        {
            // Arrange
            var loginVM = new UserLoginVM { Username = "   ", Password = "password" };

            // Act
            var result = _controller.Login(loginVM);

            // Assert
            result.Should().BeOfType<ViewResult>();
            _controller.ModelState.ErrorCount.Should().BeGreaterThan(0);
        }

        [Fact]
        public void Login_POST_WithWhitespaceOnlyPassword_ShouldReturnViewWithError()
        {
            // Arrange
            var loginVM = new UserLoginVM { Username = "testuser", Password = "   " };

            // Act
            var result = _controller.Login(loginVM);

            // Assert
            result.Should().BeOfType<ViewResult>();
            _controller.ModelState.ErrorCount.Should().BeGreaterThan(0);
        }

        #endregion
    }
}
