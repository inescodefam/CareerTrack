using CareerTrack.Data;
using CareerTrack.Models;
using CareerTrack.Security;
using CareerTrack.Services;
using CareerTrack.ViewModels;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Routing;
using Moq;
using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CareerTrack.Tests.UnitTests.Services
{
    public class AuthServiceTests
    {

        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IRoleResolver> _roleResolverMock;
        private readonly Mock<IAuthCookieService> _cookieServiceMock;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _roleResolverMock = new Mock<IRoleResolver>();
            _cookieServiceMock = new Mock<IAuthCookieService>();

            _authService = new AuthService(
                _userRepositoryMock.Object,
                _cookieServiceMock.Object,
                _roleResolverMock.Object
                );
        }

        [Fact]
        public async Task LogInAsync_ShouldReturnFailure_WhenUserDoesNotExist()
        {
            _userRepositoryMock
                .Setup(r => r.FindByUsernameAsync("Mladen"))
                .ReturnsAsync((User?)null);


            //ARRANGE
            var loginVm = new UserLoginVM
            {
                Username = "Mladen",
                Password = "password",
                ReturnUrl = "/"
            };

            //ACT
            var result = await _authService.LoginAsync(loginVm);

            //ASSERT
            result.Should().NotBeNull();
            result.success.Should().BeFalse();
            result.errorMessage.Should().NotBeNullOrWhiteSpace();
            result.reirectUrl.Should().BeNull();

            _cookieServiceMock.Verify(
                c => c.SignInAsync(It.IsAny<User>(), It.IsAny<string>()),
                Times.Never);

            _userRepositoryMock.Verify(
                u => u.FindByUsernameAsync("Mladen"), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_ShouldFail_WhenUsernameIsEmpty()
        {
            //Arrange
            var loginVm = new UserLoginVM
            {
                Username = "",
                Password = "password",
                ReturnUrl = "/"
            };

            //Act
            var result = await _authService.LoginAsync(loginVm);

            //Assert
            result.Should().NotBeNull();
            result.success.Should().BeFalse();
            result.errorMessage.Should().NotBeNullOrWhiteSpace();
            result.reirectUrl.Should().BeNull();

            _cookieServiceMock.Verify(
                c => c.SignInAsync(It.IsAny<User>(), It.IsAny<string>()),
                Times.Never);

            _userRepositoryMock.Verify(
                r => r.FindByUsernameAsync(It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task LoginAsync_ShouldFail_WhenPasswordIsEmpty()
        {
            //Arrange
            var loginVm = new UserLoginVM
            {
                Username = "mmarkic",
                Password = "",
                ReturnUrl = "/"
            };

            //Act
            var result = await _authService.LoginAsync(loginVm);

            //Assert
            result.Should().NotBeNull();
            result.success.Should().BeFalse();
            result.errorMessage.Should().NotBeNullOrWhiteSpace();
            result.reirectUrl.Should().BeNull();

            _cookieServiceMock.Verify(
                c => c.SignInAsync(It.IsAny<User>(), It.IsAny<string>()),
                Times.Never);

            _userRepositoryMock.Verify(
                r => r.FindByUsernameAsync(It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task LoginAsync_ShouldFail_WhenPasswordHashDoesNotMatch()
        {
            //Arrange
            var loginVM = new UserLoginVM
            {
                Username = "mmarkic",
                Password = "wrongPass",
                ReturnUrl = "/"
            };

            var salt = PasswordHashProvider.GetSalt();
            var correctHash = PasswordHashProvider.GetHash("password123", salt);

            var user = new User
            {
                UserName = "mmarkic",
                PasswordHash = correctHash,
                PasswordSalt = salt
            };

            _userRepositoryMock.Setup(
                u => u.FindByUsernameAsync("mmarkic"))
                .ReturnsAsync(user);


            //Act
            var result = await _authService.LoginAsync(loginVM);

            //Assert
            result.Should().NotBeNull();
            result.success.Should().BeFalse();
            result.errorMessage.Should().NotBeNullOrWhiteSpace();
            result.reirectUrl.Should().BeNull();

            _cookieServiceMock.Verify(
                c => c.SignInAsync(It.IsAny<User>(), It.IsAny<string>()),
                Times.Never);

            _userRepositoryMock.Verify(
                r => r.FindByUsernameAsync("mmarkic"),
                Times.Once);
        }

        [Fact]
        public async Task LoginAsync_ShouldSuceed_AndSignIn_WhenCredentialsAreValid()
        {
            //Arrange
            var loginVM = new UserLoginVM
            {
                Username = "mmarkic",
                Password = "password123",
                ReturnUrl = "/"
            };

            var salt = PasswordHashProvider.GetSalt();
            var correctHash = PasswordHashProvider.GetHash("password123", salt);

            var user = new User
            {
                UserName = "mmarkic",
                PasswordHash = correctHash,
                PasswordSalt = salt
            };

            _userRepositoryMock.Setup(
                u => u.FindByUsernameAsync(loginVM.Username))
                .ReturnsAsync(user);

            _roleResolverMock
                .Setup(r => r.ResolveRole(user))
                .Returns("User");


            //Act
            var result = await _authService.LoginAsync(loginVM);


            //Assert
            result.Should().NotBeNull();
            result.success.Should().BeTrue();
            result.errorMessage.Should().BeNull();
            result.reirectUrl.Should().Be("/");

            _cookieServiceMock.Verify(
                c => c.SignInAsync(user, "User"),
                Times.Once);

            _roleResolverMock.Verify(
                r => r.ResolveRole(user),
                Times.Once);
        }

        [Fact]
        public async Task LogOutAsync_ShouldCallSignOutAsync()
        {
            //Arrange

            //Act
            await _authService.LogoutAsync();

            //Assert
            _cookieServiceMock.Verify(
                c => c.SignOutAsync(), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_ShouldFail_WhenUsernameAlreadyTaken()
        {
            //Arrange
            var registerVM = new UserRegisterVM
            {
                Email = "mail@gmail.com",
                Username = "mmarkic",
                Password = "password"
            };

            _userRepositoryMock.Setup(
                u => u.ExistsByUsernameAsync(registerVM.Username))
                .ReturnsAsync(true);

            //Act
            var result = await _authService.RegisterAsync(registerVM);

            //Assert
            result.Should().NotBeNull();
            result.success.Should().BeFalse();
            result.errorMessage.Should().NotBeNullOrEmpty();
            result.reirectUrl.Should().BeNull();

            _userRepositoryMock.Verify(
                u => u.ExistsByEmailAsync(It.IsAny<string>()),
                Times.Never);

            _userRepositoryMock.Verify(
                u => u.AddAsync(It.IsAny<User>()), Times.Never);

            _userRepositoryMock.Verify(
                u => u.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task RegisterAsync_ShouldFail_WhenEmailAlreadyTaken()
        {
            //Arrange
            var registerVM = new UserRegisterVM
            {
                Email = "mmarkic@gmail.com",
                Username = "username",
                Password = "password"
            };

            _userRepositoryMock.Setup(
                u => u.ExistsByUsernameAsync(registerVM.Username))
                .ReturnsAsync(false);

            _userRepositoryMock.Setup(
                u => u.ExistsByEmailAsync(registerVM.Email))
                .ReturnsAsync(true);

            //Act
            var result = await _authService.RegisterAsync(registerVM);

            //Assert
            result.Should().NotBeNull();
            result.success.Should().BeFalse();
            result.errorMessage.Should().NotBeNullOrEmpty();
            result.reirectUrl.Should().BeNull();

            _userRepositoryMock.Verify(
                u => u.AddAsync(It.IsAny<User>()), Times.Never);

            _userRepositoryMock.Verify(
                u => u.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task RegisterAsync_ShouldSucceed_AndCreateUser_WhenDataIsValid()
        {
            //Arrange
            UserRegisterVM registerVM = new UserRegisterVM
            {
                Username = "newUser",
                Password = "password",
                Email = "mail@gmail.com",
                FirstName = "firstname",
                LastName = "lastname"
            };


            _userRepositoryMock.Setup(
                u => u.ExistsByUsernameAsync(registerVM.Username))
                .ReturnsAsync(false);

            _userRepositoryMock.Setup(
                u => u.ExistsByEmailAsync(registerVM.Email))
                .ReturnsAsync(false);

            //Act
            AuthResult result = await _authService.RegisterAsync(registerVM);


            //Assert
            result.Should().NotBeNull();
            result.success.Should().BeTrue();
            result.errorMessage.Should().BeNull();
            result.reirectUrl.Should().Be("/User/Login");

            _userRepositoryMock.Verify(
                u => u.AddAsync(It.Is<User>(user =>
                    user.UserName == "newUser" &&
                    user.Email == "mail@gmail.com" &&
                    user.FirstName == "firstname" &&
                    user.LastName == "lastname" &&
                    user.IsAdmin == false &&
                    !string.IsNullOrWhiteSpace(user.PasswordHash) &&
                    !string.IsNullOrWhiteSpace(user.PasswordSalt)
                )),
                Times.Once);

            _userRepositoryMock.Verify(
                u => u.SaveChangesAsync(), Times.Once);
        }
    }
}
