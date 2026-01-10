using System;
using CareerTrack.Models;
using CareerTrack.Utilities;
using FluentAssertions;
using Xunit;

namespace CareerTrack.Tests.UnitTests.Utilities
{
    public class DateTimeConverterTests
    {
        [Fact]
        public void ConvertToUtc_ShouldSetStartAndTargetDateKindToUtc()
        {
            //Arrange
            var converter = new DateTimeConverter();
            var goal = new Goal
            {
                startDate = new DateTime(2026, 1, 10, 12, 0, 0, DateTimeKind.Unspecified),
                targetDate = new DateTime(2026, 2, 10, 12, 0, 0, DateTimeKind.Local),
                endDate = null
            };

            //Act
            converter.ConvertToUtc(goal);

            //Assert
            goal.startDate.Kind.Should().Be(DateTimeKind.Utc);
            goal.targetDate.Kind.Should().Be(DateTimeKind.Utc);
            goal.endDate.Should().BeNull();
        }

        [Fact]
        public void ConvertToUtc_ShouldSetEndDateKindToUtc_WhenEndDateHasValue()
        {
            // Arrange
            var converter = new DateTimeConverter();
            var goal = new Goal
            {
                startDate = new DateTime(2026, 1, 10, 12, 0, 0, DateTimeKind.Unspecified),
                targetDate = new DateTime(2026, 2, 10, 12, 0, 0, DateTimeKind.Unspecified),
                endDate = new DateTime(2026, 3, 10, 12, 0, 0, DateTimeKind.Local)
            };

            // Act
            converter.ConvertToUtc(goal);

            // Assert
            goal.startDate.Kind.Should().Be(DateTimeKind.Utc);
            goal.targetDate.Kind.Should().Be(DateTimeKind.Utc);
            goal.endDate.Should().NotBeNull();
            goal.endDate!.Value.Kind.Should().Be(DateTimeKind.Utc);
        }

        [Fact]
        public void ConvertToUtc_ShouldNotChangeDateValues_OnlyKind()
        {
            //Arrange
            var converter = new DateTimeConverter();

            var start = new DateTime(2026, 1, 10, 12, 0, 0, DateTimeKind.Unspecified);
            var target = new DateTime(2026, 2, 10, 12, 0, 0, DateTimeKind.Unspecified);
            var end = new DateTime(2026, 3, 10, 12, 0, 0, DateTimeKind.Unspecified);

            var goal = new Goal
            {
                startDate = start,
                targetDate = target,
                endDate = end
            };

            //Act
            converter.ConvertToUtc(goal);

            //Assert
            goal.startDate.Ticks.Should().Be(start.Ticks);
            goal.targetDate.Ticks.Should().Be(target.Ticks);
            goal.endDate!.Value.Ticks.Should().Be(end.Ticks);

            goal.startDate.Kind.Should().Be(DateTimeKind.Utc);
            goal.targetDate.Kind.Should().Be(DateTimeKind.Utc);
            goal.endDate!.Value.Kind.Should().Be(DateTimeKind.Utc);
        }
    }
}
