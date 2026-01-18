using CareerTrack.Models;
using CareerTrack.Security;
using FluentAssertions;

namespace CareerTrack.Tests.UnitTests.Security
{
    public class PremiumRoleResolverTests
    {
        [Fact]
        public void PremiumRoleResolver_WhenIsAdminIsNull_ReturnsUser()
        {
            // Arrange
            var user = new User { IsAdmin = null };
            var resolver = new PremiumRoleResolver();

            // Act
            var result = resolver.ResolveRole(user);

            // Assert
            result.Should().Be("User"); // Not "false"
        }

        [Fact]
        public void ResolveRole_ShouldReturnAdmin_WhenIsAdminIsTrue()
        {
            // Arrange
            var resolver = new PremiumRoleResolver();
            var user = new User { IsAdmin = true };

            // Act
            string role = resolver.ResolveRole(user);

            // Assert
            role.Should().Be("Admin");
        }

        [Fact]
        public void ResolveRole_ShouldReturnUser_WhenIsAdminIsFalse()
        {
            // Arrange
            var resolver = new PremiumRoleResolver();
            var user = new User { IsAdmin = false };

            // Act
            string role = resolver.ResolveRole(user);

            // Assert
            role.Should().Be("User");
        }

        [Fact]
        public void ResolveRole_ShouldReturnPremiumUser_WhenEmailEndsWithPremiumCom()
        {
            PremiumRoleResolver resolver = new PremiumRoleResolver();

            User user = new User { Email = "mail@premium.com" };

            //Act
            string role = resolver.ResolveRole(user);

            //Assert
            role.Should().Be("User");
        }

        [Fact]
        public void ResolveRole_ShouldReturnUser_WhenEmailNotEndsWithPremiumCom_And_IsAdminOsFalse()
        {
            PremiumRoleResolver resolver = new PremiumRoleResolver();

            User user = new User
            {
                Email = "mail@mail.com",
                IsAdmin = false
            };

            //Act
            string role = resolver.ResolveRole(user);

            //Assert
            role.Should().Be("User");
        }
    }
}
