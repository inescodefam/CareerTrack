using CareerTrack.Handlers;
using CareerTrack.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CareerTrack.Tests.UnitTests.Handlers
{
    public class GoalBusinessRuleHandlerTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly GoalBusinessRuleHandler _handler;

        public GoalBusinessRuleHandlerTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _handler = new GoalBusinessRuleHandler(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region Business Rule Success Tests

        [Fact]
        public void Handle_WithCreateActionAndLessThan10ActiveGoals_ShouldReturnSuccess()
        {
            // Arrange
            var userId = 1;
            AddActiveGoalsForUser(userId, 5);

            var request = new GoalRequest
            {
                Goal = new Goal
                {
                    Name = "New Goal",
                    startDate = DateTime.UtcNow,
                    targetDate = DateTime.UtcNow.AddDays(30)
                },
                UserId = userId,
                Action = "Create"
            };

            // Act
            var result = _handler.Handle(request);

            // Assert
            result.Success.Should().BeTrue();
        }

        [Fact]
        public void Handle_WithCreateActionAndExactly9ActiveGoals_ShouldReturnSuccess()
        {
            // Arrange
            var userId = 1;
            AddActiveGoalsForUser(userId, 9);

            var request = new GoalRequest
            {
                Goal = new Goal
                {
                    Name = "10th Goal",
                    startDate = DateTime.UtcNow,
                    targetDate = DateTime.UtcNow.AddDays(30)
                },
                UserId = userId,
                Action = "Create"
            };

            // Act
            var result = _handler.Handle(request);

            // Assert
            result.Success.Should().BeTrue();
        }

        [Fact]
        public void Handle_WithNoActiveGoals_ShouldReturnSuccess()
        {
            // Arrange
            var userId = 1;

            var request = new GoalRequest
            {
                Goal = new Goal
                {
                    Name = "First Goal",
                    startDate = DateTime.UtcNow,
                    targetDate = DateTime.UtcNow.AddDays(30)
                },
                UserId = userId,
                Action = "Create"
            };

            // Act
            var result = _handler.Handle(request);

            // Assert
            result.Success.Should().BeTrue();
        }

        #endregion

        #region Business Rule Failure Tests

        [Fact]
        public void Handle_WithCreateActionAnd10ActiveGoals_ShouldReturnFailure()
        {
            // Arrange
            var userId = 1;
            AddActiveGoalsForUser(userId, 10);

            var request = new GoalRequest
            {
                Goal = new Goal
                {
                    Name = "11th Goal",
                    startDate = DateTime.UtcNow,
                    targetDate = DateTime.UtcNow.AddDays(30)
                },
                UserId = userId,
                Action = "Create"
            };

            // Act
            var result = _handler.Handle(request);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("You cannot have more than 10 active goals");
        }

        [Fact]
        public void Handle_WithCreateActionAndMoreThan10ActiveGoals_ShouldReturnFailure()
        {
            // Arrange
            var userId = 1;
            AddActiveGoalsForUser(userId, 15);

            var request = new GoalRequest
            {
                Goal = new Goal
                {
                    Name = "Another Goal",
                    startDate = DateTime.UtcNow,
                    targetDate = DateTime.UtcNow.AddDays(30)
                },
                UserId = userId,
                Action = "Create"
            };

            // Act
            var result = _handler.Handle(request);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("You cannot have more than 10 active goals");
        }

        #endregion

        #region Active vs Completed Goals Tests

        [Fact]
        public void Handle_WithCompletedGoals_ShouldNotCountTowardsLimit()
        {
            // Arrange
            var userId = 1;
            AddActiveGoalsForUser(userId, 5);
            AddCompletedGoalsForUser(userId, 20); // Many completed goals

            var request = new GoalRequest
            {
                Goal = new Goal
                {
                    Name = "New Goal",
                    startDate = DateTime.UtcNow,
                    targetDate = DateTime.UtcNow.AddDays(30)
                },
                UserId = userId,
                Action = "Create"
            };

            // Act
            var result = _handler.Handle(request);

            // Assert
            result.Success.Should().BeTrue();
        }

        [Fact]
        public void Handle_With10ActiveAnd10CompletedGoals_ShouldReturnFailure()
        {
            // Arrange
            var userId = 1;
            AddActiveGoalsForUser(userId, 10);
            AddCompletedGoalsForUser(userId, 10);

            var request = new GoalRequest
            {
                Goal = new Goal
                {
                    Name = "New Goal",
                    startDate = DateTime.UtcNow,
                    targetDate = DateTime.UtcNow.AddDays(30)
                },
                UserId = userId,
                Action = "Create"
            };

            // Act
            var result = _handler.Handle(request);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("You cannot have more than 10 active goals");
        }

        #endregion

        #region Multiple Users Tests

        [Fact]
        public void Handle_ShouldOnlyCountGoalsForSpecificUser()
        {
            // Arrange
            AddActiveGoalsForUser(1, 10); // User 1 has 10 active goals
            AddActiveGoalsForUser(2, 5);  // User 2 has 5 active goals

            var request = new GoalRequest
            {
                Goal = new Goal
                {
                    Name = "New Goal",
                    startDate = DateTime.UtcNow,
                    targetDate = DateTime.UtcNow.AddDays(30)
                },
                UserId = 2, // User 2 creating a goal
                Action = "Create"
            };

            // Act
            var result = _handler.Handle(request);

            // Assert
            result.Success.Should().BeTrue(); // User 2 should be able to create more goals
        }

        [Fact]
        public void Handle_WithDifferentUsersAtLimit_ShouldEnforcePerUserLimit()
        {
            // Arrange
            AddActiveGoalsForUser(1, 10);
            AddActiveGoalsForUser(2, 10);

            var requestUser1 = new GoalRequest
            {
                Goal = new Goal { Name = "Goal", startDate = DateTime.UtcNow, targetDate = DateTime.UtcNow.AddDays(30) },
                UserId = 1,
                Action = "Create"
            };

            var requestUser2 = new GoalRequest
            {
                Goal = new Goal { Name = "Goal", startDate = DateTime.UtcNow, targetDate = DateTime.UtcNow.AddDays(30) },
                UserId = 2,
                Action = "Create"
            };

            // Act
            var resultUser1 = _handler.Handle(requestUser1);
            var resultUser2 = _handler.Handle(requestUser2);

            // Assert
            resultUser1.Success.Should().BeFalse();
            resultUser2.Success.Should().BeFalse();
        }

        #endregion

        #region Action Type Tests

        [Theory]
        [InlineData("Update")]
        [InlineData("Delete")]
        [InlineData("View")]
        [InlineData("")]
        public void Handle_WithNonCreateAction_ShouldBypassBusinessRule(string action)
        {
            // Arrange
            var userId = 1;
            AddActiveGoalsForUser(userId, 10);

            var request = new GoalRequest
            {
                Goal = new Goal
                {
                    Id = 1,
                    Name = "Existing Goal",
                    startDate = DateTime.UtcNow,
                    targetDate = DateTime.UtcNow.AddDays(30)
                },
                UserId = userId,
                Action = action
            };

            // Act
            var result = _handler.Handle(request);

            // Assert
            result.Success.Should().BeTrue();
        }

        #endregion

        #region Chain of Responsibility Tests

        [Fact]
        public void Handle_WhenBusinessRuleViolated_ShouldNotCallNextHandler()
        {
            // Arrange
            var userId = 1;
            AddActiveGoalsForUser(userId, 10);

            var nextHandlerCalled = false;
            var nextHandler = new TestNextHandler(() => nextHandlerCalled = true);
            _handler.SetNext(nextHandler);

            var request = new GoalRequest
            {
                Goal = new Goal
                {
                    Name = "New Goal",
                    startDate = DateTime.UtcNow,
                    targetDate = DateTime.UtcNow.AddDays(30)
                },
                UserId = userId,
                Action = "Create"
            };

            // Act
            _handler.Handle(request);

            // Assert
            nextHandlerCalled.Should().BeFalse();
        }

        [Fact]
        public void Handle_WhenBusinessRulePasses_ShouldCallNextHandler()
        {
            // Arrange
            var userId = 1;
            AddActiveGoalsForUser(userId, 5);

            var nextHandlerCalled = false;
            var nextHandler = new TestNextHandler(() => nextHandlerCalled = true);
            _handler.SetNext(nextHandler);

            var request = new GoalRequest
            {
                Goal = new Goal
                {
                    Name = "New Goal",
                    startDate = DateTime.UtcNow,
                    targetDate = DateTime.UtcNow.AddDays(30)
                },
                UserId = userId,
                Action = "Create"
            };

            // Act
            _handler.Handle(request);

            // Assert
            nextHandlerCalled.Should().BeTrue();
        }

        #endregion

        #region Helper Methods

        private void AddActiveGoalsForUser(int userId, int count)
        {
            var goals = new List<Goal>();
            for (int i = 0; i < count; i++)
            {
                goals.Add(new Goal
                {
                    Name = $"Active Goal {i + 1}",
                    UserId = userId,
                    startDate = DateTime.UtcNow,
                    targetDate = DateTime.UtcNow.AddDays(30),
                    endDate = null // Active goal
                });
            }
            _context.Goals.AddRange(goals);
            _context.SaveChanges();
        }

        private void AddCompletedGoalsForUser(int userId, int count)
        {
            var goals = new List<Goal>();
            for (int i = 0; i < count; i++)
            {
                goals.Add(new Goal
                {
                    Name = $"Completed Goal {i + 1}",
                    UserId = userId,
                    startDate = DateTime.UtcNow.AddDays(-60),
                    targetDate = DateTime.UtcNow.AddDays(-30),
                    endDate = DateTime.UtcNow.AddDays(-30) // Completed goal
                });
            }
            _context.Goals.AddRange(goals);
            _context.SaveChanges();
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