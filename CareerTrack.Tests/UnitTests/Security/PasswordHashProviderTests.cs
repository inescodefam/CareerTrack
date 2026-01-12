using CareerTrack.Security;

namespace CareerTrack.Tests.UnitTests.Security
{
    public class PasswordHashProviderTests
    {
        [Fact]
        public void GetSalt_ShouldReturnNonEmptyString()
        {
            // Act
            var salt = PasswordHashProvider.GetSalt();

            // Assert
            Assert.NotNull(salt);
            Assert.NotEmpty(salt);
        }

        [Fact]
        public void GetSalt_ShouldReturnBase64String()
        {
            // Act
            var salt = PasswordHashProvider.GetSalt();

            // Assert - Should be able to convert from Base64
            var exception = Record.Exception(() => Convert.FromBase64String(salt));
            Assert.Null(exception);
        }

        [Fact]
        public void GetSalt_ShouldReturnDifferentSaltsOnMultipleCalls()
        {
            // Act
            var salt1 = PasswordHashProvider.GetSalt();
            var salt2 = PasswordHashProvider.GetSalt();
            var salt3 = PasswordHashProvider.GetSalt();

            // Assert
            Assert.NotEqual(salt1, salt2);
            Assert.NotEqual(salt2, salt3);
            Assert.NotEqual(salt1, salt3);
        }

        [Fact]
        public void GetSalt_ShouldReturn16ByteSalt()
        {
            // Act
            var salt = PasswordHashProvider.GetSalt();
            var saltBytes = Convert.FromBase64String(salt);

            // Assert - 128 bits / 8 = 16 bytes
            Assert.Equal(16, saltBytes.Length);
        }

        [Fact]
        public void GetHash_WithValidPasswordAndSalt_ShouldReturnNonEmptyHash()
        {
            // Arrange
            var password = "TestPassword123!";
            var salt = PasswordHashProvider.GetSalt();

            // Act
            var hash = PasswordHashProvider.GetHash(password, salt);

            // Assert
            Assert.NotNull(hash);
            Assert.NotEmpty(hash);
        }

        [Fact]
        public void GetHash_ShouldReturnBase64String()
        {
            // Arrange
            var password = "TestPassword123!";
            var salt = PasswordHashProvider.GetSalt();

            // Act
            var hash = PasswordHashProvider.GetHash(password, salt);

            // Assert - Should be able to convert from Base64
            var exception = Record.Exception(() => Convert.FromBase64String(hash));
            Assert.Null(exception);
        }

        [Fact]
        public void GetHash_ShouldReturn32ByteHash()
        {
            // Arrange
            var password = "TestPassword123!";
            var salt = PasswordHashProvider.GetSalt();

            // Act
            var hash = PasswordHashProvider.GetHash(password, salt);
            var hashBytes = Convert.FromBase64String(hash);

            // Assert - 256 bits / 8 = 32 bytes
            Assert.Equal(32, hashBytes.Length);
        }

        [Fact]
        public void GetHash_WithSamePasswordAndSalt_ShouldReturnSameHash()
        {
            // Arrange
            var password = "TestPassword123!";
            var salt = PasswordHashProvider.GetSalt();

            // Act
            var hash1 = PasswordHashProvider.GetHash(password, salt);
            var hash2 = PasswordHashProvider.GetHash(password, salt);

            // Assert
            Assert.Equal(hash1, hash2);
        }

        [Fact]
        public void GetHash_WithSamePasswordButDifferentSalt_ShouldReturnDifferentHash()
        {
            // Arrange
            var password = "TestPassword123!";
            var salt1 = PasswordHashProvider.GetSalt();
            var salt2 = PasswordHashProvider.GetSalt();

            // Act
            var hash1 = PasswordHashProvider.GetHash(password, salt1);
            var hash2 = PasswordHashProvider.GetHash(password, salt2);

            // Assert
            Assert.NotEqual(hash1, hash2);
        }

        [Fact]
        public void GetHash_WithDifferentPasswordsButSameSalt_ShouldReturnDifferentHash()
        {
            // Arrange
            var password1 = "TestPassword123!";
            var password2 = "DifferentPassword456!";
            var salt = PasswordHashProvider.GetSalt();

            // Act
            var hash1 = PasswordHashProvider.GetHash(password1, salt);
            var hash2 = PasswordHashProvider.GetHash(password2, salt);

            // Assert
            Assert.NotEqual(hash1, hash2);
        }

        [Fact]
        public void GetHash_WithEmptyPassword_ShouldStillReturnHash()
        {
            // Arrange
            var password = "";
            var salt = PasswordHashProvider.GetSalt();

            // Act
            var hash = PasswordHashProvider.GetHash(password, salt);

            // Assert
            Assert.NotNull(hash);
            Assert.NotEmpty(hash);
        }

        [Theory]
        [InlineData("short")]
        [InlineData("VeryLongPasswordWithManyCharacters123456789!@#$%^&*()")]
        [InlineData("Pass@123")]
        [InlineData("Contains Spaces")]
        [InlineData("Special!@#$%^&*()")]
        public void GetHash_WithVariousPasswords_ShouldReturnConsistentHashForSameSalt(string password)
        {
            // Arrange
            var salt = PasswordHashProvider.GetSalt();

            // Act
            var hash1 = PasswordHashProvider.GetHash(password, salt);
            var hash2 = PasswordHashProvider.GetHash(password, salt);

            // Assert
            Assert.Equal(hash1, hash2);
        }
    }
}