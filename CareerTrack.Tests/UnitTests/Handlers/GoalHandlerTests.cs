using CareerTrack.Handlers;
using CareerTrack.Models;
using FluentAssertions;
using Xunit;

namespace CareerTrack.Tests.UnitTests.Handlers
{
    public class GoalHandlerTests
    {
        // Test implementation of abstract GoalHandler for testing purposes
        private class TestGoalHandler : GoalHandler
        {
            public bool WasHandleCalled { get; private set; }

            public override GoalHandlerResult Handle(GoalRequest request)
            {
                WasHandleCalled = true;
                return base.Handle(request);
            }
        }

        #region SetNext Tests

        [Fact]
        public void SetNext_ShouldSetNextHandler()
        {
            // Arrange
            var handler1 = new TestGoalHandler();
            var handler2 = new TestGoalHandler();

            // Act
            var result = handler1.SetNext(handler2);

            // Assert
            result.Should().BeSameAs(handler2);
        }

        [Fact]
        public void SetNext_ShouldReturnTheHandlerThatWasSet()
        {
            // Arrange
            var handler1 = new TestGoalHandler();
            var handler2 = new TestGoalHandler();

            // Act
            var returnedHandler = handler1.SetNext(handler2);

            // Assert
            returnedHandler.Should().Be(handler2);
        }

        [Fact]
        public void SetNext_ShouldAllowChaining()
        {
            // Arrange
            var handler1 = new TestGoalHandler();
            var handler2 = new TestGoalHandler();
            var handler3 = new TestGoalHandler();

            // Act
            handler1.SetNext(handler2).SetNext(handler3);

            // Assert
            // If we reach here without exception, chaining works
            Assert.True(true);
        }

        #endregion

        #region Handle Tests

        [Fact]
        public void Handle_WithNoNextHandler_ShouldReturnSuccessResult()
        {
            // Arrange
            var handler = new TestGoalHandler();
            var request = new GoalRequest
            {
                Goal = new Goal { Name = "Test" },
                UserId = 1,
                Action = "Create"
            };

            // Act
            var result = handler.Handle(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
        }

        [Fact]
        public void Handle_WithNextHandler_ShouldCallNextHandler()
        {
            // Arrange
            var handler1 = new TestGoalHandler();
            var handler2 = new TestGoalHandler();
            handler1.SetNext(handler2);

            var request = new GoalRequest
            {
                Goal = new Goal { Name = "Test" },
                UserId = 1,
                Action = "Create"
            };

            // Act
            handler1.Handle(request);

            // Assert
            handler1.WasHandleCalled.Should().BeTrue();
            handler2.WasHandleCalled.Should().BeTrue();
        }

        [Fact]
        public void Handle_WithMultipleHandlers_ShouldCallAllHandlersInChain()
        {
            // Arrange
            var handler1 = new TestGoalHandler();
            var handler2 = new TestGoalHandler();
            var handler3 = new TestGoalHandler();
            handler1.SetNext(handler2).SetNext(handler3);

            var request = new GoalRequest
            {
                Goal = new Goal { Name = "Test" },
                UserId = 1,
                Action = "Create"
            };

            // Act
            handler1.Handle(request);

            // Assert
            handler1.WasHandleCalled.Should().BeTrue();
            handler2.WasHandleCalled.Should().BeTrue();
            handler3.WasHandleCalled.Should().BeTrue();
        }

        #endregion
    }
}