using CareerTrack.ViewModels;
using System.ComponentModel.DataAnnotations;

namespace CareerTrack.Tests.UnitTests.ViewModels
{
    public class UserRegisterVMTests
    {
        [Fact]
        public void UserRegisterVM_WithValidData_ShouldPassValidation()
        {
            var timeStamp = DateTime.Now.TimeOfDay.ToString();
            // Arrange
            var model = new UserRegisterVM
            {
                Username = "testuser_" + timeStamp,
                Password = "Password123!",
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe",
                Phone = "1234567890"
            };

            // Act
            var validationResults = ValidateModel(model);

            // Assert
            Assert.Empty(validationResults);
        }

        [Fact]
        public void UserRegisterVM_WithoutUsername_ShouldFailValidation()
        {
            // Arrange
            var model = new UserRegisterVM
            {
                Username = "",
                Password = "Password123!",
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe"
            };

            // Act
            var validationResults = ValidateModel(model);

            // Assert
            Assert.NotEmpty(validationResults);
            Assert.Contains(validationResults, v => v.MemberNames.Contains("Username"));
        }

        [Fact]
        public void UserRegisterVM_WithUsernameTooShort_ShouldFailValidation()
        {
            // Arrange
            var model = new UserRegisterVM
            {
                Username = "a", // Min length is 2
                Password = "Password123!",
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe"
            };

            // Act
            var validationResults = ValidateModel(model);

            // Assert
            Assert.NotEmpty(validationResults);
            Assert.Contains(validationResults, v => v.MemberNames.Contains("Username"));
        }

        [Fact]
        public void UserRegisterVM_WithPasswordTooShort_ShouldFailValidation()
        {
            // Arrange
            var model = new UserRegisterVM
            {
                Username = "testuser",
                Password = "Pass1!", // Less than 8 characters
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe"
            };

            // Act
            var validationResults = ValidateModel(model);

            // Assert
            Assert.NotEmpty(validationResults);
            Assert.Contains(validationResults, v => v.MemberNames.Contains("Password"));
        }

        [Fact]
        public void UserRegisterVM_WithInvalidEmail_ShouldFailValidation()
        {
            // Arrange
            var model = new UserRegisterVM
            {
                Username = "testuser",
                Password = "Password123!",
                Email = "invalid-email", // Missing @ and domain
                FirstName = "John",
                LastName = "Doe"
            };

            // Act
            var validationResults = ValidateModel(model);

            // Assert
            Assert.NotEmpty(validationResults);
            Assert.Contains(validationResults, v => v.MemberNames.Contains("Email"));
        }

        [Fact]
        public void UserRegisterVM_WithoutEmail_ShouldFailValidation()
        {
            // Arrange
            var model = new UserRegisterVM
            {
                Username = "testuser",
                Password = "Password123!",
                Email = "",
                FirstName = "John",
                LastName = "Doe"
            };

            // Act
            var validationResults = ValidateModel(model);

            // Assert
            Assert.NotEmpty(validationResults);
            Assert.Contains(validationResults, v => v.MemberNames.Contains("Email"));
        }

        [Fact]
        public void UserRegisterVM_WithoutFirstName_ShouldFailValidation()
        {
            // Arrange
            var model = new UserRegisterVM
            {
                Username = "testuser",
                Password = "Password123!",
                Email = "test@example.com",
                FirstName = "",
                LastName = "Doe"
            };

            // Act
            var validationResults = ValidateModel(model);

            // Assert
            Assert.NotEmpty(validationResults);
            Assert.Contains(validationResults, v => v.MemberNames.Contains("FirstName"));
        }

        [Fact]
        public void UserRegisterVM_WithoutLastName_ShouldFailValidation()
        {
            // Arrange
            var model = new UserRegisterVM
            {
                Username = "testuser",
                Password = "Password123!",
                Email = "test@example.com",
                FirstName = "John",
                LastName = ""
            };

            // Act
            var validationResults = ValidateModel(model);

            // Assert
            Assert.NotEmpty(validationResults);
            Assert.Contains(validationResults, v => v.MemberNames.Contains("LastName"));
        }

        [Fact]
        public void UserRegisterVM_WithFirstNameTooShort_ShouldFailValidation()
        {
            // Arrange
            var model = new UserRegisterVM
            {
                Username = "testuser",
                Password = "Password123!",
                Email = "test@example.com",
                FirstName = "J", // Min length is 2
                LastName = "Doe"
            };

            // Act
            var validationResults = ValidateModel(model);

            // Assert
            Assert.NotEmpty(validationResults);
            Assert.Contains(validationResults, v => v.MemberNames.Contains("FirstName"));
        }

        [Fact]
        public void UserRegisterVM_WithLastNameTooShort_ShouldFailValidation()
        {
            // Arrange
            var model = new UserRegisterVM
            {
                Username = "testuser",
                Password = "Password123!",
                Email = "test@example.com",
                FirstName = "John",
                LastName = "D" // Min length is 2
            };

            // Act
            var validationResults = ValidateModel(model);

            // Assert
            Assert.NotEmpty(validationResults);
            Assert.Contains(validationResults, v => v.MemberNames.Contains("LastName"));
        }

        [Fact]
        public void UserRegisterVM_WithInvalidPhone_ShouldFailValidation()
        {
            // Arrange
            var model = new UserRegisterVM
            {
                Username = "testuser",
                Password = "Password123!",
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe",
                Phone = "abc123" // Invalid phone format
            };

            // Act
            var validationResults = ValidateModel(model);

            // Assert
            Assert.NotEmpty(validationResults);
            Assert.Contains(validationResults, v => v.MemberNames.Contains("Phone"));
        }

        private static List<ValidationResult> ValidateModel(object model)
        {
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(model, null, null);
            Validator.TryValidateObject(model, validationContext, validationResults, true);
            return validationResults;
        }
    }
}