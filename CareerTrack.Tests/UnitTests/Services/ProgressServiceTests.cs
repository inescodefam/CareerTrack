using CareerTrack.Models;
using CareerTrack.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace CareerTrack.Tests.UnitTests.Services
{
    public class ProgressServiceTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly ProgressService _progressService;
        private bool _disposed = false;

        public ProgressServiceTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _progressService = new ProgressService(_context);
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
        #region InitializeProgress Tests

        [Fact]
        public void InitializeProgress_ShouldCreateProgressRecord()
        {
            // Arrange
            var goalId = 1;
            var userId = 1;

            // Act
            _progressService.InitializeProgress(goalId, userId);

            // Assert
            var progress = _context.GoalProgress.FirstOrDefault(p => p.GoalId == goalId && p.UserId == userId);
            progress.Should().NotBeNull();
            progress!.progressData.ProgressPercentage.Should().Be(0);
            progress.Notes.Should().Be("Goal created");
        }

        [Fact]
        public void InitializeProgress_ShouldSetLastUpdatedToNow()
        {
            // Arrange
            var goalId = 1;
            var userId = 1;
            var before = DateTime.UtcNow;

            // Act
            _progressService.InitializeProgress(goalId, userId);
            var after = DateTime.UtcNow;

            // Assert
            var progress = _context.GoalProgress.FirstOrDefault(p => p.GoalId == goalId && p.UserId == userId);
            progress!.progressData.LastUpdated.Should().BeOnOrAfter(before);
            progress.progressData.LastUpdated.Should().BeOnOrBefore(after);
        }

        [Fact]
        public void InitializeProgress_ShouldPersistToDatabase()
        {
            // Arrange
            var goalId = 5;
            var userId = 3;

            // Act
            _progressService.InitializeProgress(goalId, userId);

            // Assert
            _context.GoalProgress.Should().HaveCount(1);
        }

        #endregion

        #region GetProgress Tests

        [Fact]
        public void GetProgress_WithExistingProgress_ShouldReturnProgress()
        {
            // Arrange
            var goalId = 1;
            var userId = 1;
            _progressService.InitializeProgress(goalId, userId);

            // Act
            var result = _progressService.GetProgress(goalId, userId);

            // Assert
            result.Should().NotBeNull();
            result!.GoalId.Should().Be(goalId);
            result.UserId.Should().Be(userId);
        }

        [Fact]
        public void GetProgress_WithNonExistingProgress_ShouldReturnNull()
        {
            // Arrange
            var goalId = 999;
            var userId = 1;

            // Act
            var result = _progressService.GetProgress(goalId, userId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void GetProgress_ShouldReturnProgressForCorrectUser()
        {
            // Arrange
            var goalId = 1;
            var user1Id = 1;
            var user2Id = 2;
            _progressService.InitializeProgress(goalId, user1Id);

            // Act
            var result = _progressService.GetProgress(goalId, user2Id);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region UpdateProgress Tests

        [Fact]
        public void UpdateProgress_ShouldUpdateProgressPercentage()
        {
            // Arrange
            var goalId = 1;
            var userId = 1;
            _progressService.InitializeProgress(goalId, userId);

            // Act
            _progressService.UpdateProgress(goalId, userId, 50);

            // Assert
            var progress = _progressService.GetProgress(goalId, userId);
            progress!.progressData.ProgressPercentage.Should().Be(50);
        }

        [Fact]
        public void UpdateProgress_ShouldUpdateLastUpdatedTimestamp()
        {
            // Arrange
            var goalId = 1;
            var userId = 1;
            _progressService.InitializeProgress(goalId, userId);
            var before = DateTime.UtcNow;

            // Act
            _progressService.UpdateProgress(goalId, userId, 75);
            var after = DateTime.UtcNow;

            // Assert
            var progress = _progressService.GetProgress(goalId, userId);
            progress!.progressData.LastUpdated.Should().BeOnOrAfter(before);
            progress.progressData.LastUpdated.Should().BeOnOrBefore(after);
        }

        [Fact]
        public void UpdateProgress_ShouldUpdateNotes()
        {
            // Arrange
            var goalId = 1;
            var userId = 1;
            _progressService.InitializeProgress(goalId, userId);

            // Act
            _progressService.UpdateProgress(goalId, userId, 60, "Halfway there!");

            // Assert
            var progress = _progressService.GetProgress(goalId, userId);
            progress!.Notes.Should().Be("Halfway there!");
        }

        [Fact]
        public void UpdateProgress_WithNullNotes_ShouldGenerateDefaultMessage()
        {
            // Arrange
            var goalId = 1;
            var userId = 1;
            _progressService.InitializeProgress(goalId, userId);

            // Act
            _progressService.UpdateProgress(goalId, userId, 40, null);

            // Assert
            var history = _progressService.GetProgressHistory(goalId, userId);
            var latestEntry = history.First();
            latestEntry.Notes.Should().Contain("Progress updated from 0% to 40%");
        }

        [Fact]
        public void UpdateProgress_ShouldCreateHistoryEntry()
        {
            // Arrange
            var goalId = 1;
            var userId = 1;
            _progressService.InitializeProgress(goalId, userId);

            // Act
            _progressService.UpdateProgress(goalId, userId, 25);
            _progressService.UpdateProgress(goalId, userId, 50);
            _progressService.UpdateProgress(goalId, userId, 75);

            // Assert
            var history = _progressService.GetProgressHistory(goalId, userId);
            history.Should().HaveCount(4); // Initial + 3 updates
        }

        [Fact]
        public void UpdateProgress_WithPercentageLessThan0_ShouldThrowArgumentException()
        {
            // Arrange
            var goalId = 1;
            var userId = 1;
            _progressService.InitializeProgress(goalId, userId);

            // Act & Assert
            var act = () => _progressService.UpdateProgress(goalId, userId, -10);
            act.Should().Throw<ArgumentException>()
                .WithMessage("Progress must be between 0 and 100");
        }

        [Fact]
        public void UpdateProgress_WithPercentageGreaterThan100_ShouldThrowArgumentException()
        {
            // Arrange
            var goalId = 1;
            var userId = 1;
            _progressService.InitializeProgress(goalId, userId);

            // Act & Assert
            var act = () => _progressService.UpdateProgress(goalId, userId, 150);
            act.Should().Throw<ArgumentException>()
                .WithMessage("Progress must be between 0 and 100");
        }

        [Fact]
        public void UpdateProgress_WithNonExistentProgress_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var goalId = 999;
            var userId = 1;

            // Act & Assert
            var act = () => _progressService.UpdateProgress(goalId, userId, 50);
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("Progress record not found");
        }

        [Fact]
        public void UpdateProgress_With100Percent_ShouldSetGoalEndDate()
        {
            // Arrange
            var goalId = 1;
            var userId = 1;
            var goal = new Goal
            {
                Id = goalId,
                Name = "Test Goal",
                UserId = userId,
                startDate = DateTime.UtcNow,
                targetDate = DateTime.UtcNow.AddDays(30),
                endDate = null
            };
            _context.Goals.Add(goal);
            _context.SaveChanges();
            _progressService.InitializeProgress(goalId, userId);

            var before = DateTime.UtcNow;

            // Act
            _progressService.UpdateProgress(goalId, userId, 100);
            var after = DateTime.UtcNow;

            // Assert
            var updatedGoal = _context.Goals.Find(goalId);
            updatedGoal!.endDate.Should().NotBeNull();
            updatedGoal.endDate!.Value.Should().BeOnOrAfter(before);
            updatedGoal.endDate.Value.Should().BeOnOrBefore(after);
        }

        [Fact]
        public void UpdateProgress_With100Percent_WhenAlreadyCompleted_ShouldNotChangeEndDate()
        {
            // Arrange
            var goalId = 1;
            var userId = 1;
            var existingEndDate = new DateTime(2024, 6, 15, 2, 2, 2, DateTimeKind.Utc);
            var goal = new Goal
            {
                Id = goalId,
                Name = "Test Goal",
                UserId = userId,
                startDate = DateTime.UtcNow,
                targetDate = DateTime.UtcNow.AddDays(30),
                endDate = existingEndDate
            };
            _context.Goals.Add(goal);
            _context.SaveChanges();
            _progressService.InitializeProgress(goalId, userId);

            // Act
            _progressService.UpdateProgress(goalId, userId, 100);

            // Assert
            var updatedGoal = _context.Goals.Find(goalId);
            updatedGoal!.endDate.Should().Be(existingEndDate);
        }

        [Fact]
        public void UpdateProgress_WithLessThan100_ShouldNotSetEndDate()
        {
            // Arrange
            var goalId = 1;
            var userId = 1;
            var goal = new Goal
            {
                Id = goalId,
                Name = "Test Goal",
                UserId = userId,
                startDate = DateTime.UtcNow,
                targetDate = DateTime.UtcNow.AddDays(30),
                endDate = null
            };
            _context.Goals.Add(goal);
            _context.SaveChanges();
            _progressService.InitializeProgress(goalId, userId);

            // Act
            _progressService.UpdateProgress(goalId, userId, 75);

            // Assert
            var updatedGoal = _context.Goals.Find(goalId);
            updatedGoal!.endDate.Should().BeNull();
        }

        #endregion

        #region GetProgressHistory Tests

        [Fact]
        public void GetProgressHistory_ShouldReturnAllHistoryForGoal()
        {
            // Arrange
            var goalId = 1;
            var userId = 1;
            _progressService.InitializeProgress(goalId, userId);
            _progressService.UpdateProgress(goalId, userId, 25);
            _progressService.UpdateProgress(goalId, userId, 50);


            // Act
            var history = _progressService.GetProgressHistory(goalId, userId);

            // Assert
            history.Should().HaveCount(3); // Initial + 2 updates
        }

        [Fact]
        public void GetProgressHistory_ShouldReturnHistoryInDescendingOrder()
        {
            // Arrange
            var goalId = 1;
            var userId = 1;
            _progressService.InitializeProgress(goalId, userId);
            _progressService.UpdateProgress(goalId, userId, 25);
            _progressService.UpdateProgress(goalId, userId, 50);
            _progressService.UpdateProgress(goalId, userId, 75);

            // Act
            var history = _progressService.GetProgressHistory(goalId, userId).ToList();

            // Assert
            // UpdateProgress creates history entries AND updates the main record
            // InitializeProgress creates 1 record with 0%
            // Each UpdateProgress creates a NEW history entry
            // So we have: initial record (updated to 75%) + 3 history entries (75%, 50%, 25%)
            history.Count.Should().BeGreaterThanOrEqualTo(3);
            history.Should().Contain(h => h.progressData.ProgressPercentage == 75);
            history.Should().Contain(h => h.progressData.ProgressPercentage == 50);
            history.Should().Contain(h => h.progressData.ProgressPercentage == 25);
            // Verify descending order
            for (int i = 0; i < history.Count - 1; i++)
            {
                history[i].progressData.LastUpdated.Should().BeOnOrAfter(history[i + 1].progressData.LastUpdated);
            }
        }

        [Fact]
        public void GetProgressHistory_WithNoHistory_ShouldReturnEmptyList()
        {
            // Arrange
            var goalId = 999;
            var userId = 1;

            // Act
            var history = _progressService.GetProgressHistory(goalId, userId);

            // Assert
            history.Should().BeEmpty();
        }

        [Fact]
        public void GetProgressHistory_ShouldOnlyReturnHistoryForSpecificUser()
        {
            // Arrange
            var goalId = 1;
            var user1Id = 1;
            var user2Id = 2;
            _progressService.InitializeProgress(goalId, user1Id);
            _progressService.InitializeProgress(goalId, user2Id);
            _progressService.UpdateProgress(goalId, user1Id, 50);
            _progressService.UpdateProgress(goalId, user2Id, 75);

            // Act
            var historyUser1 = _progressService.GetProgressHistory(goalId, user1Id);
            var historyUser2 = _progressService.GetProgressHistory(goalId, user2Id);

            // Assert
            historyUser1.Should().HaveCount(2); // Initial + 1 update for user1
            historyUser2.Should().HaveCount(2); // Initial + 1 update for user2
            historyUser1.All(h => h.UserId == user1Id).Should().BeTrue();
            historyUser2.All(h => h.UserId == user2Id).Should().BeTrue();
        }

        #endregion
    }
}