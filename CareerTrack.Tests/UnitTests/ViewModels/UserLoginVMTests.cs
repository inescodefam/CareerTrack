using CareerTrack.ViewModels;
using System.ComponentModel.DataAnnotations;

namespace CareerTrack.Tests.UnitTests.ViewModels
{
    public class UserLoginVMTests
    {
        [Fact]
        public void UserLoginVM_WithValidData_ShouldPassValidation()
        {
            // Arrange
            var model = new UserLoginVM
            {
                Username = "testuser",
                Password = "Password123!"
            };

            // Act
            var validationResults = ValidateModel(model);

            // Assert
            Assert.Empty(validationResults);
        }

        [Fact]
        public void UserLoginVM_WithoutUsername_ShouldFailValidation()
        {
            // Arrange
            var model = new UserLoginVM
            {
                Username = "",
                Password = "Password123!"
            };

            // Act
            var validationResults = ValidateModel(model);

            // Assert
            Assert.NotEmpty(validationResults);
            Assert.Contains(validationResults, v => v.MemberNames.Contains("Username"));
        }

        [Fact]
        public void UserLoginVM_WithoutPassword_ShouldFailValidation()
        {
            // Arrange
            var model = new UserLoginVM
            {
                Username = "testuser",
                Password = ""
            };

            // Act
            var validationResults = ValidateModel(model);

            // Assert
            Assert.NotEmpty(validationResults);
            Assert.Contains(validationResults, v => v.MemberNames.Contains("Password"));
        }

        [Fact]
        public void UserLoginVM_WithNullUsername_ShouldFailValidation()
        {
            // Arrange
            var model = new UserLoginVM
            {
                Username = null!,
                Password = "Password123!"
            };

            // Act
            var validationResults = ValidateModel(model);

            // Assert
            Assert.NotEmpty(validationResults);
            Assert.Contains(validationResults, v => v.MemberNames.Contains("Username"));
        }

        [Fact]
        public void UserLoginVM_WithNullPassword_ShouldFailValidation()
        {
            // Arrange
            var model = new UserLoginVM
            {
                Username = "testuser",
                Password = null!
            };

            // Act
            var validationResults = ValidateModel(model);

            // Assert
            Assert.NotEmpty(validationResults);
            Assert.Contains(validationResults, v => v.MemberNames.Contains("Password"));
        }

        [Fact]
        public void UserLoginVM_ShouldHaveReturnUrlProperty()
        {
            // Arrange & Act
            var model = new UserLoginVM
            {
                ReturnUrl = "/Goals"
            };

            // Assert
            Assert.Equal("/Goals", model.ReturnUrl);
        }

        [Fact]
        public void UserLoginVM_WithNullReturnUrl_ShouldBeValid()
        {
            // Arrange
            var model = new UserLoginVM
            {
                Username = "testuser",
                Password = "Password123!",
                ReturnUrl = null
            };

            // Act
            var validationResults = ValidateModel(model);

            // Assert - ReturnUrl is optional
            Assert.Empty(validationResults);
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