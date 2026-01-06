using CareerTrack.Models;
using CareerTrack.Repository;
using CareerTrack.Services;
using CareerTrack.Utilities;
using FluentAssertions;
using Moq;
using Xunit;

namespace CareerTrack.Tests.UnitTests.Services
{
    public class GoalServiceTests
    {
        private readonly Mock<IGoalRepository> _mockRepository;
        private readonly Mock<IDateTimeConverter> _mockDateConverter;
        private readonly Mock<IProgressService> _mockProgressService;
        private readonly GoalService _goalService;

        public GoalServiceTests()
        {
            _mockRepository = new Mock<IGoalRepository>();
            _mockDateConverter = new Mock<IDateTimeConverter>();
            _mockProgressService = new Mock<IProgressService>();
            _goalService = new GoalService(
                _mockRepository.Object,
                _mockDateConverter.Object,
                _mockProgressService.Object
            );
        }

        #region CreateGoal Tests

        [Fact]
        public void CreateGoal_ShouldSetUserIdAndConvertToUtc()
        {
            // Arrange
            var goal = new Goal
            {
                Name = "Test Goal",
                Description = "Test Description",
                startDate = DateTime.Now,
                targetDate = DateTime.Now.AddDays(30)
            };
            var userId = 1;
            var expectedGoal = new Goal
            {
                Id = 1,
                Name = goal.Name,
                Description = goal.Description,
                startDate = goal.startDate,
                targetDate = goal.targetDate,
                UserId = userId
            };

            _mockRepository.Setup(r => r.Create(It.IsAny<Goal>())).Returns(expectedGoal);
            _mockDateConverter.Setup(d => d.ConvertToUtc(It.IsAny<Goal>()));
            _mockProgressService.Setup(p => p.InitializeProgress(It.IsAny<int>(), It.IsAny<int>()));

            // Act
            var result = _goalService.CreateGoal(goal, userId);

            // Assert
            result.Should().NotBeNull();
            result.UserId.Should().Be(userId);
            _mockDateConverter.Verify(d => d.ConvertToUtc(goal), Times.Once);
            _mockRepository.Verify(r => r.Create(goal), Times.Once);
            _mockProgressService.Verify(p => p.InitializeProgress(expectedGoal.Id, userId), Times.Once);
        }

        [Fact]
        public void CreateGoal_ShouldInitializeProgressForNewGoal()
        {
            // Arrange
            var goal = new Goal
            {
                Name = "Test Goal",
                startDate = DateTime.Now,
                targetDate = DateTime.Now.AddDays(30)
            };
            var userId = 1;
            var createdGoal = new Goal { Id = 10, Name = goal.Name, UserId = userId };

            _mockRepository.Setup(r => r.Create(It.IsAny<Goal>())).Returns(createdGoal);

            // Act
            _goalService.CreateGoal(goal, userId);

            // Assert
            _mockProgressService.Verify(p => p.InitializeProgress(10, userId), Times.Once);
        }

        [Fact]
        public void CreateGoal_ShouldReturnCreatedGoal()
        {
            // Arrange
            var goal = new Goal
            {
                Name = "Test Goal",
                startDate = DateTime.Now,
                targetDate = DateTime.Now.AddDays(30)
            };
            var userId = 1;
            var createdGoal = new Goal
            {
                Id = 5,
                Name = goal.Name,
                UserId = userId,
                startDate = goal.startDate,
                targetDate = goal.targetDate
            };

            _mockRepository.Setup(r => r.Create(It.IsAny<Goal>())).Returns(createdGoal);

            // Act
            var result = _goalService.CreateGoal(goal, userId);

            // Assert
            result.Should().BeEquivalentTo(createdGoal);
        }

        #endregion

        #region GetUserGoals Tests

        [Fact]
        public void GetUserGoals_ShouldReturnAllGoalsForUser()
        {
            // Arrange
            var userId = 1;
            var expectedGoals = new List<Goal>
            {
                new Goal { Id = 1, Name = "Goal 1", UserId = userId },
                new Goal { Id = 2, Name = "Goal 2", UserId = userId },
                new Goal { Id = 3, Name = "Goal 3", UserId = userId }
            };

            _mockRepository.Setup(r => r.GetByUserId(userId)).Returns(expectedGoals);

            // Act
            var result = _goalService.GetUserGoals(userId);

            // Assert
            result.Should().BeEquivalentTo(expectedGoals);
            result.Should().HaveCount(3);
            _mockRepository.Verify(r => r.GetByUserId(userId), Times.Once);
        }

        [Fact]
        public void GetUserGoals_ShouldReturnEmptyListWhenNoGoalsExist()
        {
            // Arrange
            var userId = 1;
            _mockRepository.Setup(r => r.GetByUserId(userId)).Returns(new List<Goal>());

            // Act
            var result = _goalService.GetUserGoals(userId);

            // Assert
            result.Should().BeEmpty();
            _mockRepository.Verify(r => r.GetByUserId(userId), Times.Once);
        }

        #endregion

        #region GetGoalById Tests

        [Fact]
        public void GetGoalById_ShouldReturnGoalWhenExists()
        {
            // Arrange
            var goalId = 1;
            var userId = 1;
            var expectedGoal = new Goal { Id = goalId, Name = "Test Goal", UserId = userId };

            _mockRepository.Setup(r => r.GetByIdAndUser(goalId, userId)).Returns(expectedGoal);

            // Act
            var result = _goalService.GetGoalById(goalId, userId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedGoal);
            _mockRepository.Verify(r => r.GetByIdAndUser(goalId, userId), Times.Once);
        }

        [Fact]
        public void GetGoalById_ShouldReturnNullWhenGoalDoesNotExist()
        {
            // Arrange
            var goalId = 999;
            var userId = 1;

            _mockRepository.Setup(r => r.GetByIdAndUser(goalId, userId)).Returns((Goal?)null);

            // Act
            var result = _goalService.GetGoalById(goalId, userId);

            // Assert
            result.Should().BeNull();
            _mockRepository.Verify(r => r.GetByIdAndUser(goalId, userId), Times.Once);
        }

        [Fact]
        public void GetGoalById_ShouldReturnNullWhenGoalBelongsToDifferentUser()
        {
            // Arrange
            var goalId = 1;
            var userId = 1;

            _mockRepository.Setup(r => r.GetByIdAndUser(goalId, userId)).Returns((Goal?)null);

            // Act
            var result = _goalService.GetGoalById(goalId, userId);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region UpdateGoal Tests

        [Fact]
        public void UpdateGoal_ShouldConvertToUtcAndUpdateGoal()
        {
            // Arrange
            var userId = 1;
            var goal = new Goal
            {
                Id = 1,
                Name = "Updated Goal",
                Description = "Updated Description",
                startDate = DateTime.Now,
                targetDate = DateTime.Now.AddDays(30),
                UserId = userId
            };

            var existingGoal = new Goal { Id = 1, Name = "Old Goal", UserId = userId };

            _mockRepository.Setup(r => r.GetByIdAndUser(goal.Id, userId)).Returns(existingGoal);
            _mockRepository.Setup(r => r.Update(goal)).Returns(goal);
            _mockDateConverter.Setup(d => d.ConvertToUtc(It.IsAny<Goal>()));

            // Act
            var result = _goalService.UpdateGoal(goal, userId);

            // Assert
            result.Should().BeEquivalentTo(goal);
            _mockDateConverter.Verify(d => d.ConvertToUtc(goal), Times.Once);
            _mockRepository.Verify(r => r.Update(goal), Times.Once);
        }

        [Fact]
        public void UpdateGoal_ShouldThrowUnauthorizedAccessExceptionWhenGoalDoesNotBelongToUser()
        {
            // Arrange
            var userId = 1;
            var goal = new Goal
            {
                Id = 1,
                Name = "Updated Goal",
                UserId = userId
            };

            _mockRepository.Setup(r => r.GetByIdAndUser(goal.Id, userId)).Returns((Goal?)null);

            // Act & Assert
            var act = () => _goalService.UpdateGoal(goal, userId);
            act.Should().Throw<UnauthorizedAccessException>();
            _mockRepository.Verify(r => r.Update(It.IsAny<Goal>()), Times.Never);
        }

        [Fact]
        public void UpdateGoal_ShouldReturnUpdatedGoal()
        {
            // Arrange
            var userId = 1;
            var goal = new Goal
            {
                Id = 1,
                Name = "Updated Goal",
                startDate = DateTime.Now,
                targetDate = DateTime.Now.AddDays(30),
                UserId = userId
            };
            var existingGoal = new Goal { Id = 1, Name = "Old Goal", UserId = userId };

            _mockRepository.Setup(r => r.GetByIdAndUser(goal.Id, userId)).Returns(existingGoal);
            _mockRepository.Setup(r => r.Update(goal)).Returns(goal);

            // Act
            var result = _goalService.UpdateGoal(goal, userId);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("Updated Goal");
        }

        #endregion

        #region DeleteGoal Tests

        [Fact]
        public void DeleteGoal_ShouldReturnTrueWhenGoalExistsAndIsDeleted()
        {
            // Arrange
            var goalId = 1;
            var userId = 1;
            var existingGoal = new Goal { Id = goalId, Name = "Test Goal", UserId = userId };

            _mockRepository.Setup(r => r.GetByIdAndUser(goalId, userId)).Returns(existingGoal);
            _mockRepository.Setup(r => r.Delete(goalId)).Returns(true);

            // Act
            var result = _goalService.DeleteGoal(goalId, userId);

            // Assert
            result.Should().BeTrue();
            _mockRepository.Verify(r => r.GetByIdAndUser(goalId, userId), Times.Once);
            _mockRepository.Verify(r => r.Delete(goalId), Times.Once);
        }

        [Fact]
        public void DeleteGoal_ShouldReturnFalseWhenGoalDoesNotExist()
        {
            // Arrange
            var goalId = 999;
            var userId = 1;

            _mockRepository.Setup(r => r.GetByIdAndUser(goalId, userId)).Returns((Goal?)null);

            // Act
            var result = _goalService.DeleteGoal(goalId, userId);

            // Assert
            result.Should().BeFalse();
            _mockRepository.Verify(r => r.GetByIdAndUser(goalId, userId), Times.Once);
            _mockRepository.Verify(r => r.Delete(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public void DeleteGoal_ShouldReturnFalseWhenGoalBelongsToDifferentUser()
        {
            // Arrange
            var goalId = 1;
            var userId = 1;

            _mockRepository.Setup(r => r.GetByIdAndUser(goalId, userId)).Returns((Goal?)null);

            // Act
            var result = _goalService.DeleteGoal(goalId, userId);

            // Assert
            result.Should().BeFalse();
            _mockRepository.Verify(r => r.Delete(It.IsAny<int>()), Times.Never);
        }

        #endregion
    }
}