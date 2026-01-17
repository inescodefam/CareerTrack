using CareerTrack.Tests.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace CareerTrack.Tests.IntegrationTests.Auth
{
    public class AuthIntegrationTests
    {
        private static FormUrlEncodedContent RegisterForm(
            string username,
            string password,
            string email,
            string firstName = "FN",
            string lastName = "LN",
            string phone = "")
        {
            return new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string,string>("Username", username),
                new KeyValuePair<string,string>("Password", password),
                new KeyValuePair<string,string>("Email", email),
                new KeyValuePair<string,string>("FirstName", firstName),
                new KeyValuePair<string,string>("LastName", lastName),
                new KeyValuePair<string,string>("Phone", phone),
            });
        }

        private static FormUrlEncodedContent LoginForm(string username, string password, string returnUrl = "/")
        {
            return new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string,string>("Username", username),
                new KeyValuePair<string,string>("Password", password),
                new KeyValuePair<string,string>("ReturnUrl", returnUrl),
            });
        }

        [Fact]
        public async Task Register_Post_ShouldCreateUser_AndRedirectToLogin()
        {
            //Arrange
            var factory = new CustomWebApplicationFactory(Guid.NewGuid().ToString("N"));
            var client = factory.CreateClient(new() { AllowAutoRedirect = false });

            var username = "newUserTest";
            var email = "newusertest@mail.com";

            //Act
            var response = await client.PostAsync("/User/Register",
                RegisterForm(username, "password123", email, "Integration", "Test", "+385912345678"));

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
            response.Headers.Location!.ToString().Should().Contain("/User/Login");


            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var created = await db.Users.FirstOrDefaultAsync(u => u.UserName == username);
            created.Should().NotBeNull();
            created!.Email.Should().Be(email);
            created.PasswordHash.Should().NotBeNullOrWhiteSpace();
            created.PasswordSalt.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task Register_Post_ShouldReturnOk_WhenUsernameAlreadyTaken()
        {
            // Arrange
            var factory = new CustomWebApplicationFactory(Guid.NewGuid().ToString("N"));
            var client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
                BaseAddress = new Uri("https://localhost")
            });

            AuthSeed.SeedUser(factory, username: "taken", password: "password123", email: "taken@mail.com");

            // Act
            var response = await client.PostAsync("/User/Register",
                RegisterForm("taken", "password123", "other@mail.com", "A", "B"));

            var body = await response.Content.ReadAsStringAsync();
            Console.WriteLine(body);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            (await db.Users.CountAsync(u => u.UserName == "taken")).Should().Be(1);
        }


        [Fact]
        public async Task Login_Post_ShouldRedirect_AndIssueCookie_WhenCredentialsAreValid()
        {
            // Arrange
            var factory = new CustomWebApplicationFactory(Guid.NewGuid().ToString("N"));
            var client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
                BaseAddress = new Uri("https://localhost")
            });

            AuthSeed.SeedUser(factory, username: "mmarkic", password: "password123", email: "mmarkic@mail.com");

            // Act
            var response = await client.PostAsync("/User/Login",
                LoginForm("mmarkic", "password123", "/"));

            // Assert
            response.Headers.TryGetValues("Set-Cookie", out var setCookie).Should().BeTrue();

            setCookie!.Should().Contain(c => c.StartsWith(".CareerTrack.Auth="));

            var authCookie = setCookie!.First(c => c.StartsWith(".CareerTrack.Auth="));

            authCookie.ToLowerInvariant().Should().Contain("httponly");
            authCookie.ToLowerInvariant().Should().Contain("secure");
            authCookie.ToLowerInvariant().Should().Contain("samesite");


        }


        
        [Fact]
        public async Task Login_Post_ShouldReturnOk_AndNotIssueAuthCookie_WhenPasswordIsInvalid()
        {
            // Arrange
            var factory = new CustomWebApplicationFactory(Guid.NewGuid().ToString("N"));
            var client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
                BaseAddress = new Uri("https://localhost")
            });

            AuthSeed.SeedUser(factory, username: "mmarkic2", password: "password123", email: "mmarkic2@mail.com");

            // Act
            var response = await client.PostAsync("/User/Login",
                LoginForm("mmarkic2", "wrongPass", "/"));

            // Debug (ako ikad opet dobiješ 500)
            var body = await response.Content.ReadAsStringAsync();
            Console.WriteLine(body);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            response.Headers.TryGetValues("Set-Cookie", out var cookies);

            (cookies ?? Array.Empty<string>())
                .Should()
                .NotContain(c => c.StartsWith(".CareerTrack.Auth="));
        }



        [Fact]
        public async Task Logout_Get_ShouldRedirectToLogin()
        {
            //Arrange
            var factory = new CustomWebApplicationFactory(Guid.NewGuid().ToString("N"));
            var client = factory.CreateClient(new() { AllowAutoRedirect = false });


            AuthSeed.SeedUser(factory, username: "logout_user", password: "password123", email: "logout@mail.com");
            var loginResp = await client.PostAsync("/User/Login", LoginForm("logout_user", "password123", "/"));
            loginResp.StatusCode.Should().Be(HttpStatusCode.Redirect);

            //Act
            var logoutResp = await client.GetAsync("/User/Logout");

            //Assert
            logoutResp.StatusCode.Should().Be(HttpStatusCode.Redirect);
            logoutResp.Headers.Location!.ToString().Should().Contain("/User/Login");
        }
    }
}
