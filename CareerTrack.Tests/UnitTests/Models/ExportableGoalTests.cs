using CareerTrack.Models;
using CareerTrack.Services.ExporterData;
using FluentAssertions;

namespace CareerTrack.Tests.UnitTests.Models
{
    public class ExportableGoalTests
    {

        [Fact]
        public void ExportableGoal_ShouldWrapGoalCorrectly()
        {
            // Arrange
            var goal = new Goal
            {
                Id = 1,
                Name = "Test Goal",
                Description = "Test Description",
                startDate = new DateTime(2024, 1, 1, 2, 2, 2, DateTimeKind.Utc),
                targetDate = new DateTime(2024, 12, 31, 2, 2, 2, DateTimeKind.Utc),
                UserId = 1
            };

            // Act
            var exportableGoal = new ExportableGoal(goal);

            // Assert
            exportableGoal.Should().NotBeNull();
        }

        [Fact]
        public void ExportableGoal_GetGoalTitle_ShouldReturnGoalName()
        {
            // Arrange
            var goal = new Goal
            {
                Name = "Complete Project X",
                startDate = DateTime.UtcNow,
                targetDate = DateTime.UtcNow.AddDays(30)
            };
            var exportableGoal = new ExportableGoal(goal);

            // Act
            var title = ((IExportGoalData)exportableGoal).getGoalTitle();

            // Assert
            title.Should().Be("Complete Project X");
        }

        [Fact]
        public void ExportableGoal_GetGoalStartDate_ShouldReturnFormattedDate()
        {
            // Arrange
            var startDate = new DateTime(2024, 6, 15, 10, 30, 0, DateTimeKind.Utc);
            var goal = new Goal
            {
                Name = "Test",
                startDate = startDate,
                targetDate = startDate.AddDays(30)
            };
            var exportableGoal = new ExportableGoal(goal);

            // Act
            var startDateString = ((IExportGoalData)exportableGoal).getGoalStartDate();

            // Assert
            startDateString.Should().Contain("2024");
            startDateString.Should().Contain("6");
            startDateString.Should().Contain("15");
        }

        [Fact]
        public void ExportableGoal_GetGoalTargetDate_ShouldReturnFormattedDate()
        {
            // Arrange
            var targetDate = new DateTime(2024, 12, 25, 15, 45, 0, DateTimeKind.Utc);
            var goal = new Goal
            {
                Name = "Test",
                startDate = DateTime.UtcNow,
                targetDate = targetDate
            };
            var exportableGoal = new ExportableGoal(goal);

            // Act
            var targetDateString = ((IExportGoalData)exportableGoal).getGoalTargetDate();

            // Assert
            targetDateString.Should().Contain("2024");
            targetDateString.Should().Contain("12");
            targetDateString.Should().Contain("25");
        }

        [Fact]
        public void ExportableGoal_ShouldImplementIExportGoalData()
        {
            // Arrange
            var goal = new Goal
            {
                Name = "Test Goal",
                startDate = DateTime.UtcNow,
                targetDate = DateTime.UtcNow.AddDays(30)
            };

            // Act
            var exportableGoal = new ExportableGoal(goal);

            // Assert
            exportableGoal.Should().BeAssignableTo<IExportGoalData>();
        }

        [Fact]
        public void ExportableGoal_WithDifferentGoals_ShouldReturnDifferentData()
        {
            // Arrange
            var goal1 = new Goal
            {
                Name = "Goal One",
                startDate = new DateTime(2024, 1, 1, 2, 2, 2, DateTimeKind.Utc),
                targetDate = new DateTime(2024, 6, 30, 2, 2, 2, DateTimeKind.Utc)
            };
            var goal2 = new Goal
            {
                Name = "Goal Two",
                startDate = new DateTime(2024, 7, 1, 2, 2, 2, DateTimeKind.Utc),
                targetDate = new DateTime(2024, 12, 31, 2, 2, 2, DateTimeKind.Utc)
            };

            var exportable1 = new ExportableGoal(goal1);
            var exportable2 = new ExportableGoal(goal2);

            // Act
            var title1 = ((IExportGoalData)exportable1).getGoalTitle();
            var title2 = ((IExportGoalData)exportable2).getGoalTitle();

            // Assert
            title1.Should().Be("Goal One");
            title2.Should().Be("Goal Two");
            title1.Should().NotBe(title2);
        }
    }
}