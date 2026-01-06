using CareerTrack.Models;
using FluentAssertions;
using Xunit;

namespace CareerTrack.Tests.UnitTests.Models
{
    public class SkillGoalTests
    {
        [Fact]
        public void SkillGoal_ShouldInheritFromGoal()
        {
            // Arrange & Act
            var skillGoal = new SkillGoal();

            // Assert
            skillGoal.Should().BeAssignableTo<Goal>();
        }

        [Fact]
        public void SkillGoal_ShouldHaveSkillCategoryProperty()
        {
            // Arrange & Act
            var skillGoal = new SkillGoal
            {
                Name = "Learn C#",
                SkillCategory = "Programming",
                ProficiencyLevel = 3,
                startDate = DateTime.UtcNow,
                targetDate = DateTime.UtcNow.AddDays(90)
            };

            // Assert
            skillGoal.SkillCategory.Should().Be("Programming");
        }

        [Fact]
        public void SkillGoal_ShouldHaveProficiencyLevelProperty()
        {
            // Arrange & Act
            var skillGoal = new SkillGoal
            {
                Name = "Master TypeScript",
                SkillCategory = "Frontend Development",
                ProficiencyLevel = 5,
                startDate = DateTime.UtcNow,
                targetDate = DateTime.UtcNow.AddDays(180)
            };

            // Assert
            skillGoal.ProficiencyLevel.Should().Be(5);
        }

        [Fact]
        public void SkillGoal_ShouldSupportAllGoalProperties()
        {
            // Arrange & Act
            var skillGoal = new SkillGoal
            {
                Id = 1,
                Name = "Learn Python",
                Description = "Learn Python for data science",
                SkillCategory = "Data Science",
                ProficiencyLevel = 4,
                startDate = new DateTime(2024, 1, 1),
                targetDate = new DateTime(2024, 6, 30),
                UserId = 10
            };

            // Assert
            skillGoal.Id.Should().Be(1);
            skillGoal.Name.Should().Be("Learn Python");
            skillGoal.Description.Should().Be("Learn Python for data science");
            skillGoal.SkillCategory.Should().Be("Data Science");
            skillGoal.ProficiencyLevel.Should().Be(4);
            skillGoal.UserId.Should().Be(10);
        }

        [Theory]
        [InlineData("Web Development", 1)]
        [InlineData("Mobile Development", 2)]
        [InlineData("Cloud Computing", 3)]
        [InlineData("DevOps", 4)]
        [InlineData("Machine Learning", 5)]
        public void SkillGoal_ShouldAcceptDifferentCategoriesAndLevels(string category, int level)
        {
            // Arrange & Act
            var skillGoal = new SkillGoal
            {
                Name = $"Learn {category}",
                SkillCategory = category,
                ProficiencyLevel = level,
                startDate = DateTime.UtcNow,
                targetDate = DateTime.UtcNow.AddDays(60)
            };

            // Assert
            skillGoal.SkillCategory.Should().Be(category);
            skillGoal.ProficiencyLevel.Should().Be(level);
        }

        [Fact]
        public void SkillGoal_CanBeCreatedWithEndDate()
        {
            // Arrange & Act
            var endDate = new DateTime(2024, 12, 31);
            var skillGoal = new SkillGoal
            {
                Name = "Complete Certification",
                SkillCategory = "Certification",
                ProficiencyLevel = 5,
                startDate = DateTime.UtcNow,
                targetDate = DateTime.UtcNow.AddDays(180),
                endDate = endDate
            };

            // Assert
            skillGoal.endDate.Should().Be(endDate);
        }
    }
}