using CareerTrack.Models;
using FluentAssertions;

namespace CareerTrack.Tests.UnitTests.Models
{
    public class GoalProgressTests
    {
        [Fact]
        public void GoalProgress_GetProgressDescription_WithNoProgressData_ShouldReturnNoProgressYet()
        {
            // Arrange
            var goalProgress = new GoalProgress
            {
                Id = 1,
                GoalId = 1,
                UserId = 1,
                progressData = null
            };

            // Act
            var result = goalProgress.GetProgressDescription();

            // Assert
            result.Should().Be("No progress yet.");
        }

        [Fact]
        public void GoalProgress_GetProgressDescription_WithProgressData_ShouldReturnPercentage()
        {
            // Arrange
            var goalProgress = new GoalProgress
            {
                Id = 1,
                GoalId = 1,
                UserId = 1,
                progressData = new GoalProgressData
                {
                    id = 1,
                    ProgressPercentage = 50,
                    LastUpdated = DateTime.UtcNow
                }
            };

            // Act
            var result = goalProgress.GetProgressDescription();

            // Assert
            result.Should().Contain("Progress: 50%");
        }

        [Fact]
        public void GoalProgress_ShouldImplementIGoalProgress()
        {
            // Arrange
            var goalProgress = new GoalProgress();

            // Assert
            goalProgress.Should().BeAssignableTo<IGoalProgress>();
        }

        [Fact]
        public void GoalProgress_ShouldInheritFromGoalProgressBase()
        {
            // Arrange
            var goalProgress = new GoalProgress();

            // Assert
            goalProgress.Should().BeAssignableTo<GoalProgressBase>();
        }
    }

    public class GoalProgressDataTests
    {
        [Fact]
        public void GoalProgressData_ShouldStoreProgressPercentage()
        {
            // Arrange & Act
            var progressData = new GoalProgressData
            {
                id = 1,
                ProgressPercentage = 75,
                LastUpdated = DateTime.UtcNow
            };

            // Assert
            progressData.ProgressPercentage.Should().Be(75);
        }

        [Fact]
        public void GoalProgressData_ShouldStoreLastUpdatedDate()
        {
            // Arrange
            var now = DateTime.UtcNow;

            // Act
            var progressData = new GoalProgressData
            {
                id = 1,
                ProgressPercentage = 50,
                LastUpdated = now
            };

            // Assert
            progressData.LastUpdated.Should().Be(now);
        }
    }

    public class GoalTrackableProgressTests
    {
        [Fact]
        public void GoalTrackableProgress_GetProgressDescription_WithNoProgressData_ShouldShowMilestonesOnly()
        {
            // Arrange
            var trackableProgress = new GoalTrackableProgress
            {
                Id = 1,
                GoalId = 1,
                UserId = 1,
                MilestonesCompleted = 2,
                TotalMilestones = 5,
                progressData = null
            };

            // Act
            var result = trackableProgress.GetProgressDescription();

            // Assert
            result.Should().Contain("Milestones Completed: 2/5");
            result.Should().Contain("no progress data yet");
        }

        [Fact]
        public void GoalTrackableProgress_GetProgressDescription_WithProgressData_ShouldShowMilestonesAndPercentage()
        {
            // Arrange
            var trackableProgress = new GoalTrackableProgress
            {
                Id = 1,
                GoalId = 1,
                UserId = 1,
                MilestonesCompleted = 3,
                TotalMilestones = 5,
                progressData = new GoalProgressData
                {
                    id = 1,
                    ProgressPercentage = 60,
                    LastUpdated = DateTime.UtcNow
                }
            };

            // Act
            var result = trackableProgress.GetProgressDescription();

            // Assert
            result.Should().Contain("Milestones Completed: 3/5");
            result.Should().Contain("Progress: 60%");
        }

        [Fact]
        public void GoalTrackableProgress_ShouldHaveMilestonesProperties()
        {
            // Arrange & Act
            var trackableProgress = new GoalTrackableProgress
            {
                MilestonesCompleted = 4,
                TotalMilestones = 10
            };

            // Assert
            trackableProgress.MilestonesCompleted.Should().Be(4);
            trackableProgress.TotalMilestones.Should().Be(10);
        }

        [Fact]
        public void GoalTrackableProgress_ShouldImplementIGoalProgress()
        {
            // Arrange
            var trackableProgress = new GoalTrackableProgress();

            // Assert
            trackableProgress.Should().BeAssignableTo<IGoalProgress>();
        }

        [Fact]
        public void GoalTrackableProgress_ShouldInheritFromGoalProgressBase()
        {
            // Arrange
            var trackableProgress = new GoalTrackableProgress();

            // Assert
            trackableProgress.Should().BeAssignableTo<GoalProgressBase>();
        }
    }

    public class GoalCompletionProgressStatusTests
    {
        [Fact]
        public void GoalCompletionProgressStatus_GetProgressDescription_WhenNotCompleted_WithNoProgressData_ShouldReturnNotCompleted()
        {
            // Arrange
            var completionStatus = new GoalCompletionProgressStatus
            {
                Id = 1,
                GoalId = 1,
                UserId = 1,
                IsCompleted = false,
                progressData = null
            };

            // Act
            var result = completionStatus.GetProgressDescription();

            // Assert
            result.Should().Be("Goal not completed yet. No progress data.");
        }

        [Fact]
        public void GoalCompletionProgressStatus_GetProgressDescription_WhenNotCompleted_WithProgressData_ShouldShowPercentage()
        {
            // Arrange
            var completionStatus = new GoalCompletionProgressStatus
            {
                Id = 1,
                GoalId = 1,
                UserId = 1,
                IsCompleted = false,
                progressData = new GoalProgressData
                {
                    id = 1,
                    ProgressPercentage = 80,
                    LastUpdated = DateTime.UtcNow
                }
            };

            // Act
            var result = completionStatus.GetProgressDescription();

            // Assert
            result.Should().Contain("Goal not completed yet");
            result.Should().Contain("Progress: 80%");
        }

        [Fact]
        public void GoalCompletionProgressStatus_GetProgressDescription_WhenCompleted_ShouldShowCompletionDate()
        {
            // Arrange
            var completionDate = new DateTime(2024, 6, 15, 2, 2, 2, DateTimeKind.Utc);
            var completionStatus = new GoalCompletionProgressStatus
            {
                Id = 1,
                GoalId = 1,
                UserId = 1,
                IsCompleted = true,
                CompletionDate = completionDate
            };

            // Act
            var result = completionStatus.GetProgressDescription();

            // Assert
            result.Should().Contain("Goal completed on");
            result.Should().Contain(completionDate.ToShortDateString());
        }

        [Fact]
        public void GoalCompletionProgressStatus_ShouldHaveCompletionProperties()
        {
            // Arrange
            var completionDate = DateTime.UtcNow;

            // Act
            var completionStatus = new GoalCompletionProgressStatus
            {
                IsCompleted = true,
                CompletionDate = completionDate
            };

            // Assert
            completionStatus.IsCompleted.Should().BeTrue();
            completionStatus.CompletionDate.Should().Be(completionDate);
        }

        [Fact]
        public void GoalCompletionProgressStatus_ShouldImplementIGoalProgress()
        {
            // Arrange
            var completionStatus = new GoalCompletionProgressStatus();

            // Assert
            completionStatus.Should().BeAssignableTo<IGoalProgress>();
        }

        [Fact]
        public void GoalCompletionProgressStatus_ShouldInheritFromGoalProgressBase()
        {
            // Arrange
            var completionStatus = new GoalCompletionProgressStatus();

            // Assert
            completionStatus.Should().BeAssignableTo<GoalProgressBase>();
        }
    }
}