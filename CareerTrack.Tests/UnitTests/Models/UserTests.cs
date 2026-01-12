using CareerTrack.Models;

namespace CareerTrack.Tests.UnitTests.Models
{
    public class UserTests
    {
        [Fact]
        public void User_CanBeCreated_WithAllProperties()
        {
            // Arrange & Act
            var user = new User
            {
                Id = 1,
                UserName = "testuser",
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe",
                Phone = "1234567890",
                PasswordHash = "hash123",
                PasswordSalt = "salt123",
                IsAdmin = false
            };

            // Assert
            Assert.Equal(1, user.Id);
            Assert.Equal("testuser", user.UserName);
            Assert.Equal("test@example.com", user.Email);
            Assert.Equal("John", user.FirstName);
            Assert.Equal("Doe", user.LastName);
            Assert.Equal("1234567890", user.Phone);
            Assert.Equal("hash123", user.PasswordHash);
            Assert.Equal("salt123", user.PasswordSalt);
            Assert.False(user.IsAdmin);
        }

        [Fact]
        public void User_IsAdminProperty_DefaultsToFalse()
        {
            // Arrange & Act
            var user = new User();

            // Assert
            Assert.False(user.IsAdmin);
        }

        [Fact]
        public void User_HasRequiredProperties()
        {
            // Arrange & Act
            var user = new User
            {
                Id = 1,
                UserName = "testuser",
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe",
                PasswordHash = "hash",
                PasswordSalt = "salt"
            };

            // Assert
            Assert.NotNull(user.UserName);
            Assert.NotNull(user.Email);
            Assert.NotNull(user.FirstName);
            Assert.NotNull(user.LastName);
            Assert.NotNull(user.PasswordHash);
            Assert.NotNull(user.PasswordSalt);
        }

        [Fact]
        public void User_AsAdmin_ShouldHaveIsAdminTrue()
        {
            // Arrange & Act
            var admin = new User
            {
                UserName = "adminuser",
                Email = "admin@example.com",
                FirstName = "Admin",
                LastName = "User",
                PasswordHash = "hash",
                PasswordSalt = "salt",
                IsAdmin = true
            };

            // Assert
            Assert.True(admin.IsAdmin);
        }

        [Fact]
        public void User_AsRegularUser_ShouldHaveIsAdminFalse()
        {
            // Arrange & Act
            var user = new User
            {
                UserName = "regularuser",
                Email = "user@example.com",
                FirstName = "Regular",
                LastName = "User",
                PasswordHash = "hash",
                PasswordSalt = "salt",
                IsAdmin = false
            };

            // Assert
            Assert.False(user.IsAdmin);
        }

        [Fact]
        public void User_WithNullPhone_ShouldBeValid()
        {
            // Arrange & Act
            var user = new User
            {
                UserName = "testuser",
                Email = "test@example.com",
                FirstName = "Test",
                LastName = "User",
                PasswordHash = "hash",
                PasswordSalt = "salt",
                Phone = null
            };

            // Assert
            Assert.Null(user.Phone);
        }

        [Fact]
        public void User_PropertiesCanBeModified()
        {
            // Arrange
            var user = new User
            {
                UserName = "oldusername",
                Email = "old@example.com",
                FirstName = "Old",
                LastName = "Name"
            };

            // Act
            user.UserName = "newusername";
            user.Email = "new@example.com";
            user.FirstName = "New";
            user.LastName = "Name2";

            // Assert
            Assert.Equal("newusername", user.UserName);
            Assert.Equal("new@example.com", user.Email);
            Assert.Equal("New", user.FirstName);
            Assert.Equal("Name2", user.LastName);
        }
    }
}