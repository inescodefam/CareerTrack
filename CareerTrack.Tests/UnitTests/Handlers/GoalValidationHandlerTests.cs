using CareerTrack.Handlers;
using CareerTrack.Models;
using FluentAssertions;
using Xunit;

namespace CareerTrack.Tests.UnitTests.Handlers
{
    public class GoalValidationHandlerTests
    {
        private readonly GoalValidationHandler _handler;

        public GoalValidationHandlerTests()
        {
            _handler = new GoalValidationHandler();
        }

        #region Validation Success Tests

        [Fact]
        public void Handle_WithValidGoal_ShouldReturnSuccessResult()
        {
            // Arrange
            var request = new GoalRequest
            {
                Goal = new Goal
                {
                    Name = "Valid Goal",
                    startDate = DateTime.UtcNow,
                    targetDate = DateTime.UtcNow.AddDays(30)
                },
                UserId = 1,
                Action = "Create"
            };

            // Act
            var result = _handler.Handle(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Handle_WithValidGoalMaxLength_ShouldReturnSuccessResult()
        {
            // Arrange
            var request = new GoalRequest
            {
                Goal = new Goal
                {
                    Name = new string('A', 150), // Exactly 150 characters
                    startDate = DateTime.UtcNow,
                    targetDate = DateTime.UtcNow.AddDays(30)
                },
                UserId = 1,
                Action = "Create"
            };

            // Act
            var result = _handler.Handle(request);

            // Assert
            result.Success.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        #endregion

        #region Name Validation Tests

        [Fact]
        public void Handle_WithNullName_ShouldReturnValidationError()
        {
            // Arrange
            var request = new GoalRequest
            {
                Goal = new Goal
                {
                    Name = null!,
                    startDate = DateTime.UtcNow,
                    targetDate = DateTime.UtcNow.AddDays(30)
                },
                UserId = 1,
                Action = "Create"
            };

            // Act
            var result = _handler.Handle(request);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Validation failed");
            result.Errors.Should().Contain("Goal name is required");
        }

        [Fact]
        public void Handle_WithEmptyName_ShouldReturnValidationError()
        {
            // Arrange
            var request = new GoalRequest
            {
                Goal = new Goal
                {
                    Name = "",
                    startDate = DateTime.UtcNow,
                    targetDate = DateTime.UtcNow.AddDays(30)
                },
                UserId = 1,
                Action = "Create"
            };

            // Act
            var result = _handler.Handle(request);

            // Assert
            result.Success.Should().BeFalse();
            result.Errors.Should().Contain("Goal name is required");
        }

        [Fact]
        public void Handle_WithWhitespaceName_ShouldReturnValidationError()
        {
            // Arrange
            var request = new GoalRequest
            {
                Goal = new Goal
                {
                    Name = "   ",
                    startDate = DateTime.UtcNow,
                    targetDate = DateTime.UtcNow.AddDays(30)
                },
                UserId = 1,
                Action = "Create"
            };

            // Act
            var result = _handler.Handle(request);

            // Assert
            result.Success.Should().BeFalse();
            result.Errors.Should().Contain("Goal name is required");
        }

        [Fact]
        public void Handle_WithNameLongerThan150Characters_ShouldReturnValidationError()
        {
            // Arrange
            var request = new GoalRequest
            {
                Goal = new Goal
                {
                    Name = new string('A', 151), // 151 characters
                    startDate = DateTime.UtcNow,
                    targetDate = DateTime.UtcNow.AddDays(30)
                },
                UserId = 1,
                Action = "Create"
            };

            // Act
            var result = _handler.Handle(request);

            // Assert
            result.Success.Should().BeFalse();
            result.Errors.Should().Contain("Goal name must be 150 characters or less");
        }

        #endregion

        #region Date Validation Tests

        [Fact]
        public void Handle_WithPastTargetDate_ShouldReturnValidationError()
        {
            // Arrange
            var request = new GoalRequest
            {
                Goal = new Goal
                {
                    Name = "Test Goal",
                    startDate = DateTime.UtcNow.AddDays(-30),
                    targetDate = DateTime.UtcNow.AddDays(-10)
                },
                UserId = 1,
                Action = "Create"
            };

            // Act
            var result = _handler.Handle(request);

            // Assert
            result.Success.Should().BeFalse();
            result.Errors.Should().Contain("Target date must be in the future");
        }

        [Fact]
        public void Handle_WithTargetDateEqualToNow_ShouldReturnValidationError()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var request = new GoalRequest
            {
                Goal = new Goal
                {
                    Name = "Test Goal",
                    startDate = now.AddDays(-1),
                    targetDate = now.AddSeconds(-1) // Slightly in the past
                },
                UserId = 1,
                Action = "Create"
            };

            // Act
            var result = _handler.Handle(request);

            // Assert
            result.Success.Should().BeFalse();
            result.Errors.Should().Contain("Target date must be in the future");
        }

        [Fact]
        public void Handle_WithStartDateAfterTargetDate_ShouldReturnValidationError()
        {
            // Arrange
            var request = new GoalRequest
            {
                Goal = new Goal
                {
                    Name = "Test Goal",
                    startDate = DateTime.UtcNow.AddDays(60),
                    targetDate = DateTime.UtcNow.AddDays(30)
                },
                UserId = 1,
                Action = "Create"
            };

            // Act
            var result = _handler.Handle(request);

            // Assert
            result.Success.Should().BeFalse();
            result.Errors.Should().Contain("Start date must be before target date");
        }

        [Fact]
        public void Handle_WithStartDateEqualToTargetDate_ShouldReturnValidationError()
        {
            // Arrange
            var sameDate = DateTime.UtcNow.AddDays(30);
            var request = new GoalRequest
            {
                Goal = new Goal
                {
                    Name = "Test Goal",
                    startDate = sameDate,
                    targetDate = sameDate
                },
                UserId = 1,
                Action = "Create"
            };

            // Act
            var result = _handler.Handle(request);

            // Assert
            result.Success.Should().BeFalse();
            result.Errors.Should().Contain("Start date must be before target date");
        }

        #endregion

        #region Multiple Validation Errors Tests

        [Fact]
        public void Handle_WithMultipleValidationErrors_ShouldReturnAllErrors()
        {
            // Arrange
            var request = new GoalRequest
            {
                Goal = new Goal
                {
                    Name = "", // Invalid: empty
                    startDate = DateTime.UtcNow.AddDays(60),
                    targetDate = DateTime.UtcNow.AddDays(30) // Invalid: start after target
                },
                UserId = 1,
                Action = "Create"
            };

            // Act
            var result = _handler.Handle(request);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Validation failed");
            result.Errors.Should().HaveCountGreaterThan(1);
            result.Errors.Should().Contain("Goal name is required");
        }

        [Fact]
        public void Handle_WithAllInvalidData_ShouldReturnAllValidationErrors()
        {
            // Arrange
            var request = new GoalRequest
            {
                Goal = new Goal
                {
                    Name = new string('A', 200), // Too long
                    startDate = DateTime.UtcNow.AddDays(60),
                    targetDate = DateTime.UtcNow.AddDays(-10) // Past and before start
                },
                UserId = 1,
                Action = "Create"
            };

            // Act
            var result = _handler.Handle(request);

            // Assert
            result.Success.Should().BeFalse();
            result.Errors.Should().HaveCountGreaterThanOrEqualTo(2);
        }

        #endregion

        #region Chain of Responsibility Tests

        [Fact]
        public void Handle_WhenValidationFails_ShouldNotCallNextHandler()
        {
            // Arrange
            var nextHandlerCalled = false;
            var nextHandler = new TestNextHandler(() => nextHandlerCalled = true);
            _handler.SetNext(nextHandler);

            var request = new GoalRequest
            {
                Goal = new Goal
                {
                    Name = "", // Invalid
                    startDate = DateTime.UtcNow,
                    targetDate = DateTime.UtcNow.AddDays(30)
                },
                UserId = 1,
                Action = "Create"
            };

            // Act
            _handler.Handle(request);

            // Assert
            nextHandlerCalled.Should().BeFalse();
        }

        [Fact]
        public void Handle_WhenValidationPasses_ShouldCallNextHandler()
        {
            // Arrange
            var nextHandlerCalled = false;
            var nextHandler = new TestNextHandler(() => nextHandlerCalled = true);
            _handler.SetNext(nextHandler);

            var request = new GoalRequest
            {
                Goal = new Goal
                {
                    Name = "Valid Goal",
                    startDate = DateTime.UtcNow,
                    targetDate = DateTime.UtcNow.AddDays(30)
                },
                UserId = 1,
                Action = "Create"
            };

            // Act
            _handler.Handle(request);

            // Assert
            nextHandlerCalled.Should().BeTrue();
        }

        #endregion

        // Helper class for testing chain of responsibility
        private class TestNextHandler : GoalHandler
        {
            private readonly Action _onHandleCalled;

            public TestNextHandler(Action onHandleCalled)
            {
                _onHandleCalled = onHandleCalled;
            }

            public override GoalHandlerResult Handle(GoalRequest request)
            {
                _onHandleCalled();
                return new GoalHandlerResult { Success = true };
            }
        }
    }
}