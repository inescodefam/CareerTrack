using CareerTrack.Models;
using CareerTrack.Security;
using FluentAssertions;

namespace CareerTrack.Tests.UnitTests.Security
{
    public class DefaultRoleResolverTests
    {
        [Fact]
        public void ResolveRole_ShouldReturnUser_WhenIsAdminIsFalse()
        {
            // Arrange
            var resolver = new DefaultRoleResolver();
            var user = new User { IsAdmin = false };

            // Act
            string role = resolver.ResolveRole(user);

            // Assert
            role.Should().Be("User");
        }

        [Fact]
        public void ResolveRole_ShouldReturnUser_WhenIsAdminIsNull()
        {
            // Arrange
            var resolver = new DefaultRoleResolver();
            var user = new User { IsAdmin = null };

            // Act
            string role = resolver.ResolveRole(user);

            // Assert
            role.Should().Be("User");
        }

        [Fact]
        public void ResolveRole_ShouldHandleUserWithAllProperties()
        {
            // Arrange
            var resolver = new DefaultRoleResolver();
            var user = new User
            {
                IsAdmin = true,
                UserName = "testuser",
                Email = "test@example.com",
                FirstName = "Test",
                LastName = "User",
                PasswordHash = "hash",
                PasswordSalt = "salt"
            };

            // Act
            string role = resolver.ResolveRole(user);

            // Assert
            role.Should().Be("Admin");
        }
    }
}
