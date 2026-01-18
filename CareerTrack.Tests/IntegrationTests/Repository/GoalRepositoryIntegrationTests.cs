using CareerTrack.Models;
using CareerTrack.Repository;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CareerTrack.Tests.IntegrationTests.Repository
{
    public class GoalRepositoryIntegrationTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly GoalRepository _repository;

        public GoalRepositoryIntegrationTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _repository = new GoalRepository(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region Create Tests

        [Fact]
        public void Create_ShouldAddGoalToDatabase()
        {
            // Arrange
            var goal = new Goal
            {
                Name = "Test Goal",
                Description = "Test Description",
                startDate = DateTime.UtcNow,
                targetDate = DateTime.UtcNow.AddDays(30),
                UserId = 1
            };

            // Act
            var result = _repository.Create(goal);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().BeGreaterThan(0);
            _context.Goals.Should().Contain(goal);
        }

        [Fact]
        public void Create_ShouldReturnGoalWithGeneratedId()
        {
            // Arrange
            var goal = new Goal
            {
                Name = "New Goal",
                startDate = DateTime.UtcNow,
                targetDate = DateTime.UtcNow.AddDays(30),
                UserId = 1
            };

            // Act
            var result = _repository.Create(goal);

            // Assert
            result.Id.Should().NotBe(0);
            result.Name.Should().Be("New Goal");
        }

        [Fact]
        public void Create_ShouldPersistGoalProperties()
        {
            // Arrange
            var startDate = DateTime.UtcNow;
            var targetDate = DateTime.UtcNow.AddDays(60);
            var goal = new Goal
            {
                Name = "Complete Goal",
                Description = "Description here",
                startDate = startDate,
                targetDate = targetDate,
                UserId = 5
            };

            // Act
            var created = _repository.Create(goal);
            var retrieved = _context.Goals.Find(created.Id);

            // Assert
            retrieved.Should().NotBeNull();
            retrieved!.Name.Should().Be("Complete Goal");
            retrieved.Description.Should().Be("Description here");
            retrieved.UserId.Should().Be(5);
        }

        #endregion

        #region GetById Tests

        [Fact]
        public void GetById_WithExistingGoal_ShouldReturnGoal()
        {
            // Arrange
            var user = new User { Id = 1, UserName = "Test User", FirstName = "Test User", LastName = "User", Email = "test@example.com", PasswordHash = "hash", PasswordSalt = "salt" };
            var goal = new Goal
            {
                Name = "Test Goal",
                startDate = DateTime.UtcNow,
                targetDate = DateTime.UtcNow.AddDays(30),
                UserId = user.Id!.Value
            };

            _context.Users.Add(user);
            _context.Goals.Add(goal);
            _context.SaveChanges();

            // Act
            var result = _repository.GetById(goal.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(goal.Id);
            result.Name.Should().Be("Test Goal");
        }

        [Fact]
        public void GetById_WithNonExistentGoal_ShouldReturnNull()
        {
            // Act
            var result = _repository.GetById(999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void GetById_ShouldIncludeUserNavigation()
        {
            // Arrange
            var user = new User { Id = 1, UserName = "Test User", FirstName = "Test User", LastName = "User", Email = "test@example.com", PasswordHash = "hash", PasswordSalt = "salt" };
            var goal = new Goal
            {
                Name = "Test Goal",
                startDate = DateTime.UtcNow,
                targetDate = DateTime.UtcNow.AddDays(30),
                UserId = user.Id!.Value
            };

            _context.Users.Add(user);
            _context.Goals.Add(goal);
            _context.SaveChanges();

            // Act
            var result = _repository.GetById(goal.Id);

            // Assert
            result.Should().NotBeNull();
            result!.User.Should().NotBeNull();
            result.User!.FirstName.Should().Be("Test User");
        }

        #endregion

        #region GetByIdAndUser Tests

        [Fact]
        public void GetByIdAndUser_WithMatchingGoalAndUser_ShouldReturnGoal()
        {
            // Arrange
            var user = new User { Id = 1, UserName = "User 1", FirstName = "User 1", LastName = "User", Email = "test@example.com", PasswordHash = "hash", PasswordSalt = "salt" };
            var goal = new Goal
            {
                Name = "User 1 Goal",
                startDate = DateTime.UtcNow,
                targetDate = DateTime.UtcNow.AddDays(30),
                UserId = user.Id!.Value
            };

            _context.Users.Add(user);
            _context.Goals.Add(goal);
            _context.SaveChanges();

            // Act
            var result = _repository.GetByIdAndUser(goal.Id, user.Id.Value);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(goal.Id);
            result.UserId.Should().Be(user.Id);
        }

        [Fact]
        public void GetByIdAndUser_WithDifferentUser_ShouldReturnNull()
        {
            // Arrange
            var user1 = new User { Id = 1, UserName = "User 1", FirstName = "User 1", LastName = "User", Email = "test@example.com", PasswordHash = "hash", PasswordSalt = "salt" };
            var user2 = new User { Id = 2, UserName = "User 2", FirstName = "User 2", LastName = "User", Email = "test@example.com", PasswordHash = "hash", PasswordSalt = "salt" };
            var goal = new Goal
            {
                Name = "User 1 Goal",
                startDate = DateTime.UtcNow,
                targetDate = DateTime.UtcNow.AddDays(30),
                UserId = user1.Id!.Value
            };

            _context.Users.AddRange(user1, user2);
            _context.Goals.Add(goal);
            _context.SaveChanges();

            // Act
            var result = _repository.GetByIdAndUser(goal.Id, user2.Id!.Value);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void GetByIdAndUser_WithNonExistentGoal_ShouldReturnNull()
        {
            // Act
            var result = _repository.GetByIdAndUser(999, 1);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void GetByIdAndUser_ShouldIncludeUserNavigation()
        {
            // Arrange
            var user = new User { Id = 1, UserName = "Test User", FirstName = "Test User", LastName = "User", Email = "test@example.com", PasswordHash = "hash", PasswordSalt = "salt" };
            var goal = new Goal
            {
                Name = "Test Goal",
                startDate = DateTime.UtcNow,
                targetDate = DateTime.UtcNow.AddDays(30),
                UserId = user.Id!.Value
            };

            _context.Users.Add(user);
            _context.Goals.Add(goal);
            _context.SaveChanges();

            // Act
            var result = _repository.GetByIdAndUser(goal.Id, user.Id.Value);

            // Assert
            result.Should().NotBeNull();
            result!.User.Should().NotBeNull();
            result.User!.Email.Should().Be("test@example.com");
        }

        #endregion

        #region GetByUserId Tests

        [Fact]
        public void GetByUserId_ShouldReturnAllGoalsForUser()
        {
            // Arrange
            var user = new User { Id = 1, UserName = "User 1", FirstName = "User 1", LastName = "User", Email = "test@example.com", PasswordHash = "hash", PasswordSalt = "salt" };
            var goals = new List<Goal>
            {
                new Goal { Name = "Goal 1", UserId = user.Id!.Value, startDate = DateTime.UtcNow, targetDate = DateTime.UtcNow.AddDays(10) },
                new Goal { Name = "Goal 2", UserId = user.Id.Value, startDate = DateTime.UtcNow, targetDate = DateTime.UtcNow.AddDays(20) },
                new Goal { Name = "Goal 3", UserId = user.Id.Value, startDate = DateTime.UtcNow, targetDate = DateTime.UtcNow.AddDays(30) }
            };

            _context.Users.Add(user);
            _context.Goals.AddRange(goals);
            _context.SaveChanges();

            // Act
            var result = _repository.GetByUserId(user.Id.Value);

            // Assert
            result.Should().HaveCount(3);
            result.Should().OnlyContain(g => g.UserId == user.Id);
        }

        [Fact]
        public void GetByUserId_ShouldOnlyReturnGoalsForSpecifiedUser()
        {
            // Arrange
            var user1 = new User { Id = 1, UserName = "User 1", FirstName = "User 1", LastName = "User", Email = "test@example.com", PasswordHash = "hash", PasswordSalt = "salt" };
            var user2 = new User { Id = 2, UserName = "User 2", FirstName = "User 2", LastName = "User", Email = "test@example.com", PasswordHash = "hash", PasswordSalt = "salt" };
            var goalsUser1 = new List<Goal>
            {
                new Goal { Name = "User 1 Goal 1", UserId = user1.Id!.Value, startDate = DateTime.UtcNow, targetDate = DateTime.UtcNow.AddDays(10) },
                new Goal { Name = "User 1 Goal 2", UserId = user1.Id.Value, startDate = DateTime.UtcNow, targetDate = DateTime.UtcNow.AddDays(20) }
            };
            var goalsUser2 = new List<Goal>
            {
                new Goal { Name = "User 2 Goal 1", UserId = user2.Id!.Value, startDate = DateTime.UtcNow, targetDate = DateTime.UtcNow.AddDays(15) }
            };

            _context.Users.AddRange(user1, user2);
            _context.Goals.AddRange(goalsUser1);
            _context.Goals.AddRange(goalsUser2);
            _context.SaveChanges();

            // Act
            var result = _repository.GetByUserId(user1.Id.Value);

            // Assert
            result.Should().HaveCount(2);
            result.Should().OnlyContain(g => g.UserId == user1.Id);
        }

        [Fact]
        public void GetByUserId_ShouldReturnEmptyListWhenNoGoalsExist()
        {
            // Act
            var result = _repository.GetByUserId(999);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public void GetByUserId_ShouldOrderByStartDateDescending()
        {
            // Arrange
            var user = new User { Id = 1, UserName = "User 1", FirstName = "User 1", LastName = "User", Email = "test@example.com", PasswordHash = "hash", PasswordSalt = "salt" };
            var now = DateTime.UtcNow;
            var goals = new List<Goal>
            {
                new Goal { Name = "Oldest", UserId = user.Id!.Value, startDate = now.AddDays(-30), targetDate = now.AddDays(-20) },
                new Goal { Name = "Newest", UserId = user.Id.Value, startDate = now.AddDays(-1), targetDate = now.AddDays(10) },
                new Goal { Name = "Middle", UserId = user.Id.Value, startDate = now.AddDays(-15), targetDate = now.AddDays(5) }
            };

            _context.Users.Add(user);
            _context.Goals.AddRange(goals);
            _context.SaveChanges();

            // Act
            var result = _repository.GetByUserId(user.Id.Value).ToList();

            // Assert
            result.Should().HaveCount(3);
            result[0].Name.Should().Be("Newest");
            result[1].Name.Should().Be("Middle");
            result[2].Name.Should().Be("Oldest");
        }

        [Fact]
        public void GetByUserId_ShouldIncludeUserNavigation()
        {
            // Arrange
            var user = new User { Id = 1, UserName = "Test User", FirstName = "Test User", LastName = "User", Email = "test@example.com", PasswordHash = "hash", PasswordSalt = "salt" };
            var goal = new Goal
            {
                Name = "Test Goal",
                UserId = user.Id!.Value,
                startDate = DateTime.UtcNow,
                targetDate = DateTime.UtcNow.AddDays(30)
            };

            _context.Users.Add(user);
            _context.Goals.Add(goal);
            _context.SaveChanges();

            // Act
            var result = _repository.GetByUserId(user.Id.Value).ToList();

            // Assert
            result.Should().HaveCount(1);
            result[0].User.Should().NotBeNull();
            result[0].User!.FirstName.Should().Be("Test User");
        }

        #endregion

        #region Update Tests

        [Fact]
        public async Task Update_ShouldModifyExistingGoal()
        {
            // Arrange
            var goal = new Goal
            {
                Name = "Original Name",
                Description = "Original Description",
                startDate = DateTime.UtcNow,
                targetDate = DateTime.UtcNow.AddDays(30),
                UserId = 1
            };

            _context.Goals.Add(goal);
            _context.SaveChanges();

            // Modify the goal
            goal.Name = "Updated Name";
            goal.Description = "Updated Description";

            // Act
            _repository.Update(goal);
            await Task.Delay(100); // Small delay to ensure async SaveChanges completes

            // Assert
            var updated = _context.Goals.Find(goal.Id);
            updated.Should().NotBeNull();
            updated!.Name.Should().Be("Updated Name");
            updated.Description.Should().Be("Updated Description");
        }

        [Fact]
        public async Task Update_ShouldReturnUpdatedGoal()
        {
            // Arrange
            var goal = new Goal
            {
                Name = "Original",
                startDate = DateTime.UtcNow,
                targetDate = DateTime.UtcNow.AddDays(30),
                UserId = 1
            };

            _context.Goals.Add(goal);
            _context.SaveChanges();

            goal.Name = "Modified";

            // Act
            var result = _repository.Update(goal);
            await Task.Delay(100); // Small delay to ensure async SaveChanges completes

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("Modified");
        }

        #endregion

        #region Delete Tests

        [Fact]
        public void Delete_WithExistingGoal_ShouldRemoveGoalAndReturnTrue()
        {
            // Arrange
            var goal = new Goal
            {
                Name = "Goal to Delete",
                startDate = DateTime.UtcNow,
                targetDate = DateTime.UtcNow.AddDays(30),
                UserId = 1
            };

            _context.Goals.Add(goal);
            _context.SaveChanges();

            // Act
            var result = _repository.Delete(goal.Id);

            // Assert
            result.Should().BeTrue();
            _context.Goals.Find(goal.Id).Should().BeNull();
        }

        [Fact]
        public void Delete_WithNonExistentGoal_ShouldReturnFalse()
        {
            // Act
            var result = _repository.Delete(999);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void Delete_ShouldNotAffectOtherGoals()
        {
            // Arrange
            var goal1 = new Goal { Name = "Goal 1", UserId = 1, startDate = DateTime.UtcNow, targetDate = DateTime.UtcNow.AddDays(10) };
            var goal2 = new Goal { Name = "Goal 2", UserId = 1, startDate = DateTime.UtcNow, targetDate = DateTime.UtcNow.AddDays(20) };
            var goal3 = new Goal { Name = "Goal 3", UserId = 1, startDate = DateTime.UtcNow, targetDate = DateTime.UtcNow.AddDays(30) };

            _context.Goals.AddRange(goal1, goal2, goal3);
            _context.SaveChanges();

            // Act
            _repository.Delete(goal2.Id);

            // Assert
            _context.Goals.Should().HaveCount(2);
            _context.Goals.Find(goal1.Id).Should().NotBeNull();
            _context.Goals.Find(goal2.Id).Should().BeNull();
            _context.Goals.Find(goal3.Id).Should().NotBeNull();
        }

        #endregion

        #region Integration Scenarios Tests

        [Fact]
        public void IntegrationScenario_CreateRetrieveUpdateDelete_ShouldWorkCorrectly()
        {
            // Create
            var user = new User { Id = 1, UserName = "Test User", FirstName = "Test User", LastName = "User", Email = "test@example.com", PasswordHash = "hash", PasswordSalt = "salt" };
            _context.Users.Add(user);
            _context.SaveChanges();

            var goal = new Goal
            {
                Name = "Integration Test Goal",
                Description = "Testing full lifecycle",
                startDate = DateTime.UtcNow,
                targetDate = DateTime.UtcNow.AddDays(30),
                UserId = user.Id!.Value
            };

            var created = _repository.Create(goal);
            created.Id.Should().BeGreaterThan(0);

            // Retrieve
            var retrieved = _repository.GetById(created.Id);
            retrieved.Should().NotBeNull();
            retrieved!.Name.Should().Be("Integration Test Goal");

            // Update
            retrieved.Name = "Updated Integration Test Goal";
            _repository.Update(retrieved);

            var updated = _repository.GetById(created.Id);
            updated!.Name.Should().Be("Updated Integration Test Goal");

            // Delete
            var deleteResult = _repository.Delete(created.Id);
            deleteResult.Should().BeTrue();

            var deleted = _repository.GetById(created.Id);
            deleted.Should().BeNull();
        }

        [Fact]
        public void IntegrationScenario_MultipleUsersWithGoals_ShouldIsolateUserData()
        {
            // Arrange
            var user1 = new User { Id = 1, UserName = "User 1", FirstName = "User 1", LastName = "User", Email = "test@example.com", PasswordHash = "hash", PasswordSalt = "salt" };
            var user2 = new User { Id = 2, UserName = "User 2", FirstName = "User 2", LastName = "User", Email = "test@example.com", PasswordHash = "hash", PasswordSalt = "salt" };

            _context.Users.AddRange(user1, user2);

            var user1Goals = new List<Goal>
            {
                new Goal { Name = "User 1 Goal 1", UserId = user1.Id!.Value, startDate = DateTime.UtcNow, targetDate = DateTime.UtcNow.AddDays(10) },
                new Goal { Name = "User 1 Goal 2", UserId = user1.Id.Value, startDate = DateTime.UtcNow, targetDate = DateTime.UtcNow.AddDays(20) }
            };

            var user2Goals = new List<Goal>
            {
                new Goal { Name = "User 2 Goal 1", UserId = user2.Id!.Value, startDate = DateTime.UtcNow, targetDate = DateTime.UtcNow.AddDays(15) }
            };

            _context.Goals.AddRange(user1Goals);
            _context.Goals.AddRange(user2Goals);
            _context.SaveChanges();

            // Act
            var user1Retrieved = _repository.GetByUserId(user1.Id.Value).ToList();
            var user2Retrieved = _repository.GetByUserId(user2.Id.Value).ToList();

            // Assert
            user1Retrieved.Should().HaveCount(2);
            user1Retrieved.Should().OnlyContain(g => g.UserId == user1.Id);

            user2Retrieved.Should().HaveCount(1);
            user2Retrieved.Should().OnlyContain(g => g.UserId == user2.Id);

            // Verify cross-user access returns null
            var crossAccess = _repository.GetByIdAndUser(user1Goals[0].Id, user2.Id.Value);
            crossAccess.Should().BeNull();
        }

        #endregion
    }
}