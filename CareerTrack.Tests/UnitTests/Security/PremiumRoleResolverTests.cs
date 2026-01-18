using CareerTrack.Models;
using CareerTrack.Security;
using FluentAssertions;

namespace CareerTrack.Tests.UnitTests.Security
{
    public class PremiumRoleResolverTests
    {
        [Fact]
        public void ResolveRole_WhenUserIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            var resolver = new PremiumRoleResolver();

            // Act
            var act = () => resolver.ResolveRole(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("user");
        }

        [Fact]
        public void ResolveRole_WhenIsAdminIsNull_ShouldReturnUser()
        {
            // Arrange
            var user = new User { IsAdmin = null };
            var resolver = new PremiumRoleResolver();

            // Act
            var result = resolver.ResolveRole(user);

            // Assert
            result.Should().Be("User");
        }

        [Fact]
        public void ResolveRole_WhenIsAdminIsTrue_ShouldReturnAdmin()
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
        public void ResolveRole_WhenIsAdminIsFalse_ShouldReturnUser()
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
        public void ResolveRole_WithPremiumEmail_WhenIsAdminIsNull_ShouldReturnUser()
        {
            // Arrange
            var resolver = new PremiumRoleResolver();
            var user = new User { Email = "mail@premium.com", IsAdmin = null };

            // Act
            string role = resolver.ResolveRole(user);

            // Assert
            role.Should().Be("User");
        }

        [Fact]
        public void ResolveRole_WithPremiumEmail_WhenIsAdminIsTrue_ShouldReturnAdmin()
        {
            // Arrange
            var resolver = new PremiumRoleResolver();
            var user = new User { Email = "mail@premium.com", IsAdmin = true };

            // Act
            string role = resolver.ResolveRole(user);

            // Assert
            role.Should().Be("Admin");
        }

        [Fact]
        public void ResolveRole_WithRegularEmail_WhenIsAdminIsFalse_ShouldReturnUser()
        {
            // Arrange
            var resolver = new PremiumRoleResolver();
            var user = new User
            {
                Email = "mail@mail.com",
                IsAdmin = false
            };

            // Act
            string role = resolver.ResolveRole(user);

            // Assert
            role.Should().Be("User");
        }

        [Fact]
        public void ResolveRole_WithDefaultIsAdmin_ShouldReturnUser()
        {
            // Arrange - IsAdmin defaults to false per User model
            var resolver = new PremiumRoleResolver();
            var user = new User();

            // Act
            string role = resolver.ResolveRole(user);

            // Assert
            role.Should().Be("User");
        }
    }
}
