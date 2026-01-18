using CareerTrack.Handlers;
using CareerTrack.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace CareerTrack.Tests.UnitTests.Handlers
{
    public class GoalAuthorizationHandlerTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly GoalAuthorizationHandler _handler;
        private bool _disposed;

        public GoalAuthorizationHandlerTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _handler = new GoalAuthorizationHandler(_context);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context.Database.EnsureDeleted();
                    _context.Dispose();
                }
                _disposed = true;
            }
        }

        #region Authorization Success Tests

        [Fact]
        public void Handle_WithCreateAction_ShouldReturnSuccessWithoutAuthorization()
        {
            // Arrange
            var request = new GoalRequest
            {
                Goal = new Goal
                {
                    Name = "Test Goal",
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
        }

        [Fact]
        public void Handle_WithDeleteActionAndCorrectUser_ShouldReturnSuccess()
        {
            // Arrange
            var userId = 1;
            var goal = new Goal
            {
                Id = 1,
                Name = "Test Goal",
                UserId = userId,
                startDate = DateTime.UtcNow,
                targetDate = DateTime.UtcNow.AddDays(30)
            };

            _context.Goals.Add(goal);
            _context.SaveChanges();

            var request = new GoalRequest
            {
                Goal = goal,
                UserId = userId,
                Action = "Delete"
            };

            // Act
            var result = _handler.Handle(request);

            // Assert
            result.Success.Should().BeTrue();
        }

        [Fact]
        public void Handle_WithUpdateActionAndCorrectUser_ShouldReturnSuccess()
        {
            // Arrange
            var userId = 1;
            var goal = new Goal
            {
                Id = 1,
                Name = "Test Goal",
                UserId = userId,
                startDate = DateTime.UtcNow,
                targetDate = DateTime.UtcNow.AddDays(30)
            };

            _context.Goals.Add(goal);
            _context.SaveChanges();

            var request = new GoalRequest
            {
                Goal = goal,
                UserId = userId,
                Action = "Update"
            };

            // Act
            var result = _handler.Handle(request);

            // Assert
            result.Success.Should().BeTrue();
        }

        #endregion

        #region Authorization Failure Tests

        [Fact]
        public void Handle_WithDeleteActionAndDifferentUser_ShouldReturnUnauthorized()
        {
            // Arrange
            var goal = new Goal
            {
                Id = 1,
                Name = "Test Goal",
                UserId = 1,
                startDate = DateTime.UtcNow,
                targetDate = DateTime.UtcNow.AddDays(30)
            };

            _context.Goals.Add(goal);
            _context.SaveChanges();

            var request = new GoalRequest
            {
                Goal = goal,
                UserId = 2, // Different user
                Action = "Delete"
            };

            // Act
            var result = _handler.Handle(request);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("You are not authorized to modify this goal");
        }

        [Fact]
        public void Handle_WithUpdateActionAndDifferentUser_ShouldReturnUnauthorized()
        {
            // Arrange
            var goal = new Goal
            {
                Id = 1,
                Name = "Test Goal",
                UserId = 1,
                startDate = DateTime.UtcNow,
                targetDate = DateTime.UtcNow.AddDays(30)
            };

            _context.Goals.Add(goal);
            _context.SaveChanges();

            var request = new GoalRequest
            {
                Goal = goal,
                UserId = 999, // Different user
                Action = "Update"
            };

            // Act
            var result = _handler.Handle(request);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("You are not authorized to modify this goal");
        }

        #endregion

        #region Action Type Tests

        [Theory]
        [InlineData("Create")]
        [InlineData("View")]
        [InlineData("List")]
        [InlineData("")]
        public void Handle_WithNonRestrictedAction_ShouldBypassAuthorization(string action)
        {
            // Arrange
            var request = new GoalRequest
            {
                Goal = new Goal
                {
                    Id = 1,
                    Name = "Test Goal",
                    UserId = 1,
                    startDate = DateTime.UtcNow,
                    targetDate = DateTime.UtcNow.AddDays(30)
                },
                UserId = 999, // Different user, but should bypass for non-restricted actions
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
        public void Handle_WhenUnauthorized_ShouldNotCallNextHandler()
        {
            // Arrange
            var goal = new Goal
            {
                Id = 1,
                Name = "Test Goal",
                UserId = 1,
                startDate = DateTime.UtcNow,
                targetDate = DateTime.UtcNow.AddDays(30)
            };

            _context.Goals.Add(goal);
            _context.SaveChanges();

            var nextHandlerCalled = false;
            var nextHandler = new TestNextHandler(() => nextHandlerCalled = true);
            _handler.SetNext(nextHandler);

            var request = new GoalRequest
            {
                Goal = goal,
                UserId = 2, // Different user
                Action = "Delete"
            };

            // Act
            _handler.Handle(request);

            // Assert
            nextHandlerCalled.Should().BeFalse();
        }

        [Fact]
        public void Handle_WhenAuthorized_ShouldCallNextHandler()
        {
            // Arrange
            var userId = 1;
            var goal = new Goal
            {
                Id = 1,
                Name = "Test Goal",
                UserId = userId,
                startDate = DateTime.UtcNow,
                targetDate = DateTime.UtcNow.AddDays(30)
            };

            _context.Goals.Add(goal);
            _context.SaveChanges();

            var nextHandlerCalled = false;
            var nextHandler = new TestNextHandler(() => nextHandlerCalled = true);
            _handler.SetNext(nextHandler);

            var request = new GoalRequest
            {
                Goal = goal,
                UserId = userId,
                Action = "Delete"
            };

            // Act
            _handler.Handle(request);

            // Assert
            nextHandlerCalled.Should().BeTrue();
        }

        #endregion

        #region Multiple Goals Tests

        [Fact]
        public void Handle_WithMultipleGoals_ShouldAuthorizeCorrectGoal()
        {
            // Arrange
            var goals = new List<Goal>
            {
                new Goal { Id = 1, Name = "Goal 1", UserId = 1, startDate = DateTime.UtcNow, targetDate = DateTime.UtcNow.AddDays(30) },
                new Goal { Id = 2, Name = "Goal 2", UserId = 2, startDate = DateTime.UtcNow, targetDate = DateTime.UtcNow.AddDays(30) },
                new Goal { Id = 3, Name = "Goal 3", UserId = 1, startDate = DateTime.UtcNow, targetDate = DateTime.UtcNow.AddDays(30) }
            };

            _context.Goals.AddRange(goals);
            _context.SaveChanges();

            var request = new GoalRequest
            {
                Goal = goals[2], // Goal 3, belongs to user 1
                UserId = 1,
                Action = "Update"
            };

            // Act
            var result = _handler.Handle(request);

            // Assert
            result.Success.Should().BeTrue();
        }

        [Fact]
        public void Handle_WithMultipleGoals_ShouldRejectUnauthorizedGoal()
        {
            // Arrange
            var goals = new List<Goal>
            {
                new Goal { Id = 1, Name = "Goal 1", UserId = 1, startDate = DateTime.UtcNow, targetDate = DateTime.UtcNow.AddDays(30) },
                new Goal { Id = 2, Name = "Goal 2", UserId = 2, startDate = DateTime.UtcNow, targetDate = DateTime.UtcNow.AddDays(30) },
                new Goal { Id = 3, Name = "Goal 3", UserId = 3, startDate = DateTime.UtcNow, targetDate = DateTime.UtcNow.AddDays(30) }
            };

            _context.Goals.AddRange(goals);
            _context.SaveChanges();

            var request = new GoalRequest
            {
                Goal = goals[1], // Goal 2, belongs to user 2
                UserId = 1, // User 1 trying to modify
                Action = "Delete"
            };

            // Act
            var result = _handler.Handle(request);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("You are not authorized to modify this goal");
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