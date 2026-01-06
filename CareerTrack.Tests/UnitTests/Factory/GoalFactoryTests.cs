using CareerTrack.Factory;
using CareerTrack.Models;
using FluentAssertions;
using Xunit;

namespace CareerTrack.Tests.UnitTests.Factory
{
    public class GoalFactoryTests
    {
        private readonly GoalFactory _goalFactory;

        public GoalFactoryTests()
        {
            _goalFactory = new GoalFactory();
        }

        #region CreateGoal - SkillGoal Tests

        [Fact]
        public void CreateGoal_WithSkillType_ShouldReturnSkillGoal()
        {
            // Arrange
            var goalType = "skill";
            var name = "Learn C#";
            var targetDate = DateTime.UtcNow.AddDays(30);

            // Act
            var result = _goalFactory.CreateGoal(goalType, name, targetDate);

            // Assert
            result.Should().BeOfType<SkillGoal>();
            result.Name.Should().Be(name);
            result.targetDate.Should().Be(targetDate);
            result.startDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public void CreateGoal_WithSkillTypeUpperCase_ShouldReturnSkillGoal()
        {
            // Arrange
            var goalType = "SKILL";
            var name = "Learn Python";
            var targetDate = DateTime.UtcNow.AddDays(60);

            // Act
            var result = _goalFactory.CreateGoal(goalType, name, targetDate);

            // Assert
            result.Should().BeOfType<SkillGoal>();
        }

        #endregion

        #region CreateGoal - ShortTermGoal Tests

        [Fact]
        public void CreateGoal_WithShortType_ShouldReturnShortTermGoal()
        {
            // Arrange
            var goalType = "short";
            var name = "Complete project";
            var targetDate = DateTime.UtcNow.AddDays(15);

            // Act
            var result = _goalFactory.CreateGoal(goalType, name, targetDate);

            // Assert
            result.Should().BeOfType<ShortTermGoal>();
            result.Name.Should().Be(name);
            result.targetDate.Should().Be(targetDate);
            result.startDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public void CreateGoal_WithShortType_ShouldSetReminderFrequencyTo7Days()
        {
            // Arrange
            var goalType = "short";
            var name = "Complete sprint";
            var targetDate = DateTime.UtcNow.AddDays(14);

            // Act
            var result = _goalFactory.CreateGoal(goalType, name, targetDate);

            // Assert
            var shortTermGoal = result as ShortTermGoal;
            shortTermGoal.Should().NotBeNull();
            shortTermGoal!.ReminderFrequencyDays.Should().Be(7);
        }

        #endregion

        #region CreateGoal - LongTermGoal Tests

        [Fact]
        public void CreateGoal_WithLongType_ShouldReturnLongTermGoal()
        {
            // Arrange
            var goalType = "long";
            var name = "Career advancement";
            var targetDate = DateTime.UtcNow.AddDays(365);

            // Act
            var result = _goalFactory.CreateGoal(goalType, name, targetDate);

            // Assert
            result.Should().BeOfType<LongTermGoal>();
            result.Name.Should().Be(name);
            result.targetDate.Should().Be(targetDate);
            result.startDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public void CreateGoal_WithLongType_ShouldInitializeEmptyMilestonesList()
        {
            // Arrange
            var goalType = "long";
            var name = "Become team lead";
            var targetDate = DateTime.UtcNow.AddDays(730);

            // Act
            var result = _goalFactory.CreateGoal(goalType, name, targetDate);

            // Assert
            var longTermGoal = result as LongTermGoal;
            longTermGoal.Should().NotBeNull();
            longTermGoal!.Milestones.Should().NotBeNull();
            longTermGoal.Milestones.Should().BeEmpty();
        }

        #endregion

        #region CreateGoal - Default Goal Tests

        [Fact]
        public void CreateGoal_WithUnknownType_ShouldReturnBaseGoal()
        {
            // Arrange
            var goalType = "unknown";
            var name = "Generic goal";
            var targetDate = DateTime.UtcNow.AddDays(45);

            // Act
            var result = _goalFactory.CreateGoal(goalType, name, targetDate);

            // Assert
            result.Should().BeOfType<Goal>();
            result.Should().NotBeOfType<SkillGoal>();
            result.Should().NotBeOfType<ShortTermGoal>();
            result.Should().NotBeOfType<LongTermGoal>();
            result.Name.Should().Be(name);
            result.targetDate.Should().Be(targetDate);
        }

        [Fact]
        public void CreateGoal_WithEmptyType_ShouldReturnBaseGoal()
        {
            // Arrange
            var goalType = "";
            var name = "Default goal";
            var targetDate = DateTime.UtcNow.AddDays(20);

            // Act
            var result = _goalFactory.CreateGoal(goalType, name, targetDate);

            // Assert
            result.Should().BeOfType<Goal>();
        }

        #endregion

        #region CreateGoal - Validation Tests

        [Fact]
        public void CreateGoal_WithPastTargetDate_ShouldThrowArgumentException()
        {
            // Arrange
            var goalType = "short";
            var name = "Past goal";
            var targetDate = DateTime.UtcNow.AddDays(-10);

            // Act & Assert
            var act = () => _goalFactory.CreateGoal(goalType, name, targetDate);
            act.Should().Throw<ArgumentException>()
                .WithMessage("Target date must be in the future.");
        }

        [Fact]
        public void CreateGoal_WithTargetDateInPast_ShouldThrowArgumentException()
        {
            // Arrange
            var goalType = "long";
            var name = "Old goal";
            var targetDate = DateTime.UtcNow.AddYears(-1);

            // Act & Assert
            var act = () => _goalFactory.CreateGoal(goalType, name, targetDate);
            act.Should().Throw<ArgumentException>()
                .WithMessage("Target date must be in the future.");
        }

        [Fact]
        public void CreateGoal_WithTargetDateToday_ShouldThrowArgumentException()
        {
            // Arrange
            var goalType = "short";
            var name = "Today goal";
            var targetDate = DateTime.UtcNow.AddHours(-1);

            // Act & Assert
            var act = () => _goalFactory.CreateGoal(goalType, name, targetDate);
            act.Should().Throw<ArgumentException>();
        }

        #endregion

        #region CreateGoal - StartDate Tests

        [Theory]
        [InlineData("skill")]
        [InlineData("short")]
        [InlineData("long")]
        [InlineData("default")]
        public void CreateGoal_ShouldSetStartDateToCurrentUtcTime(string goalType)
        {
            // Arrange
            var name = "Test Goal";
            var targetDate = DateTime.UtcNow.AddDays(30);
            var beforeCreation = DateTime.UtcNow;

            // Act
            var result = _goalFactory.CreateGoal(goalType, name, targetDate);
            var afterCreation = DateTime.UtcNow;

            // Assert
            result.startDate.Should().BeOnOrAfter(beforeCreation);
            result.startDate.Should().BeOnOrBefore(afterCreation);
        }

        #endregion

        #region CreateGoal - Name Tests

        [Theory]
        [InlineData("skill", "Learn ASP.NET Core")]
        [InlineData("short", "Complete unit tests")]
        [InlineData("long", "Build entire application")]
        [InlineData("other", "Some other goal")]
        public void CreateGoal_ShouldSetNameCorrectly(string goalType, string name)
        {
            // Arrange
            var targetDate = DateTime.UtcNow.AddDays(30);

            // Act
            var result = _goalFactory.CreateGoal(goalType, name, targetDate);

            // Assert
            result.Name.Should().Be(name);
        }

        #endregion

        #region CreateGoal - Mixed Case Type Tests

        [Theory]
        [InlineData("Skill")]
        [InlineData("SKILL")]
        [InlineData("sKiLl")]
        public void CreateGoal_WithMixedCaseSkillType_ShouldReturnSkillGoal(string goalType)
        {
            // Arrange
            var name = "Test";
            var targetDate = DateTime.UtcNow.AddDays(30);

            // Act
            var result = _goalFactory.CreateGoal(goalType, name, targetDate);

            // Assert
            result.Should().BeOfType<SkillGoal>();
        }

        [Theory]
        [InlineData("Short")]
        [InlineData("SHORT")]
        [InlineData("sHoRt")]
        public void CreateGoal_WithMixedCaseShortType_ShouldReturnShortTermGoal(string goalType)
        {
            // Arrange
            var name = "Test";
            var targetDate = DateTime.UtcNow.AddDays(30);

            // Act
            var result = _goalFactory.CreateGoal(goalType, name, targetDate);

            // Assert
            result.Should().BeOfType<ShortTermGoal>();
        }

        [Theory]
        [InlineData("Long")]
        [InlineData("LONG")]
        [InlineData("lOnG")]
        public void CreateGoal_WithMixedCaseLongType_ShouldReturnLongTermGoal(string goalType)
        {
            // Arrange
            var name = "Test";
            var targetDate = DateTime.UtcNow.AddDays(30);

            // Act
            var result = _goalFactory.CreateGoal(goalType, name, targetDate);

            // Assert
            result.Should().BeOfType<LongTermGoal>();
        }

        #endregion
    }
}