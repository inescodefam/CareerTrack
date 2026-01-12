using CareerTrack.Models;
using CareerTrack.Utilities;

namespace CareerTrack.Tests.UnitTests.Utilities
{
    public class DateTimeConverterTests
    {
        private readonly DateTimeConverter _converter;

        public DateTimeConverterTests()
        {
            _converter = new DateTimeConverter();
        }

        [Fact]
        public void ConvertToUtc_WhenGoalHasUnspecifiedDates_ShouldConvertToUtc()
        {
            // Arrange
            var goal = new Goal
            {
                startDate = new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Unspecified),
                targetDate = new DateTime(2024, 12, 31, 10, 0, 0, DateTimeKind.Unspecified),
                endDate = null
            };

            // Act
            _converter.ConvertToUtc(goal);

            // Assert
            Assert.Equal(DateTimeKind.Utc, goal.startDate.Kind);
            Assert.Equal(DateTimeKind.Utc, goal.targetDate.Kind);
        }

        [Fact]
        public void ConvertToUtc_WhenGoalHasEndDate_ShouldConvertEndDateToUtc()
        {
            // Arrange
            var goal = new Goal
            {
                startDate = new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Unspecified),
                targetDate = new DateTime(2024, 12, 31, 10, 0, 0, DateTimeKind.Unspecified),
                endDate = new DateTime(2024, 6, 15, 10, 0, 0, DateTimeKind.Unspecified)
            };

            // Act
            _converter.ConvertToUtc(goal);

            // Assert
            Assert.Equal(DateTimeKind.Utc, goal.startDate.Kind);
            Assert.Equal(DateTimeKind.Utc, goal.targetDate.Kind);
            Assert.Equal(DateTimeKind.Utc, goal.endDate.Value.Kind);
        }

        [Fact]
        public void ConvertToUtc_WhenEndDateIsNull_ShouldNotThrowException()
        {
            // Arrange
            var goal = new Goal
            {
                startDate = new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Unspecified),
                targetDate = new DateTime(2024, 12, 31, 10, 0, 0, DateTimeKind.Unspecified),
                endDate = null
            };

            // Act & Assert
            var exception = Record.Exception(() => _converter.ConvertToUtc(goal));
            Assert.Null(exception);
        }

        [Fact]
        public void ConvertToUtc_WhenDatesAlreadyUtc_ShouldRemainUtc()
        {
            // Arrange
            var goal = new Goal
            {
                startDate = new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Utc),
                targetDate = new DateTime(2024, 12, 31, 10, 0, 0, DateTimeKind.Utc),
                endDate = new DateTime(2024, 6, 15, 10, 0, 0, DateTimeKind.Utc)
            };

            // Act
            _converter.ConvertToUtc(goal);

            // Assert
            Assert.Equal(DateTimeKind.Utc, goal.startDate.Kind);
            Assert.Equal(DateTimeKind.Utc, goal.targetDate.Kind);
            Assert.Equal(DateTimeKind.Utc, goal.endDate.Value.Kind);
        }

        [Fact]
        public void ConvertToUtc_ShouldPreserveDateAndTimeValues()
        {
            // Arrange
            var startDate = new DateTime(2024, 1, 1, 10, 30, 45, DateTimeKind.Unspecified);
            var targetDate = new DateTime(2024, 12, 31, 15, 45, 30, DateTimeKind.Unspecified);
            var goal = new Goal
            {
                startDate = startDate,
                targetDate = targetDate,
                endDate = null
            };

            // Act
            _converter.ConvertToUtc(goal);

            // Assert - Values should be the same, only Kind changes
            Assert.Equal(startDate.Year, goal.startDate.Year);
            Assert.Equal(startDate.Month, goal.startDate.Month);
            Assert.Equal(startDate.Day, goal.startDate.Day);
            Assert.Equal(startDate.Hour, goal.startDate.Hour);
            Assert.Equal(startDate.Minute, goal.startDate.Minute);
            Assert.Equal(startDate.Second, goal.startDate.Second);
        }
    }
}