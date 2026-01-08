using CareerTrack.Models;
using CareerTrack.Services;
using CareerTrack.Services.ExporterData;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace CareerTrack.Tests.UnitTests.Services
{
    public class GoalExportServiceTests : IDisposable
    {
        private readonly Mock<IGoalService> _mockGoalService;
        private readonly Mock<IProgressService> _mockProgressService;
        private readonly Mock<IExporter> _mockPdfExporter;
        private readonly Mock<IExporter> _mockExcelExporter;
        private readonly AppDbContext _context;
        private readonly GoalExportService _goalExportService;

        public GoalExportServiceTests()
        {
            _mockGoalService = new Mock<IGoalService>();
            _mockProgressService = new Mock<IProgressService>();
            _mockPdfExporter = new Mock<IExporter>();
            _mockExcelExporter = new Mock<IExporter>();

            // Setup exporters
            _mockPdfExporter.Setup(e => e.FormatName).Returns("PDF");
            _mockPdfExporter.Setup(e => e.ContentType).Returns("application/pdf");
            _mockPdfExporter.Setup(e => e.Export(It.IsAny<IExportUserData>(), It.IsAny<IExportGoalData>()))
                .Returns(new byte[] { 1, 2, 3 });

            _mockExcelExporter.Setup(e => e.FormatName).Returns("Excel");
            _mockExcelExporter.Setup(e => e.ContentType).Returns("application/vnd.ms-excel");
            _mockExcelExporter.Setup(e => e.Export(It.IsAny<IExportUserData>(), It.IsAny<IExportGoalData>()))
                .Returns(new byte[] { 4, 5, 6 });

            var exporters = new List<IExporter> { _mockPdfExporter.Object, _mockExcelExporter.Object };

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);

            _goalExportService = new GoalExportService(
                exporters,
                _mockGoalService.Object,
                _mockProgressService.Object,
                _context
            );
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region ExportGoal Tests

        [Fact]
        public void ExportGoal_WithPdfFormat_ShouldUsePdfExporter()
        {
            // Arrange
            var goalId = 1;
            var userId = 1;
            var format = "PDF";

            var user = new User { Id = userId, UserName = "testuser", FirstName = "Test", LastName = "User", Email = "test@example.com", PasswordHash = "hash", PasswordSalt = "salt" };
            var goal = new Goal
            {
                Id = goalId,
                Name = "Test Goal",
                UserId = userId,
                startDate = DateTime.UtcNow,
                targetDate = DateTime.UtcNow.AddDays(30)
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            _mockGoalService.Setup(s => s.GetGoalById(goalId, userId)).Returns(goal);

            // Act
            var result = _goalExportService.ExportGoal(goalId, userId, format);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(new byte[] { 1, 2, 3 });
            _mockPdfExporter.Verify(e => e.Export(It.IsAny<IExportUserData>(), It.IsAny<IExportGoalData>()), Times.Once);
            _mockExcelExporter.Verify(e => e.Export(It.IsAny<IExportUserData>(), It.IsAny<IExportGoalData>()), Times.Never);
        }

        [Fact]
        public void ExportGoal_WithExcelFormat_ShouldUseExcelExporter()
        {
            // Arrange
            var goalId = 1;
            var userId = 1;
            var format = "Excel";

            var user = new User { Id = userId, UserName = "testuser", FirstName = "Test", LastName = "User", Email = "test@example.com", PasswordHash = "hash", PasswordSalt = "salt" };
            var goal = new Goal
            {
                Id = goalId,
                Name = "Test Goal",
                UserId = userId,
                startDate = DateTime.UtcNow,
                targetDate = DateTime.UtcNow.AddDays(30)
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            _mockGoalService.Setup(s => s.GetGoalById(goalId, userId)).Returns(goal);

            // Act
            var result = _goalExportService.ExportGoal(goalId, userId, format);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(new byte[] { 4, 5, 6 });
            _mockExcelExporter.Verify(e => e.Export(It.IsAny<IExportUserData>(), It.IsAny<IExportGoalData>()), Times.Once);
            _mockPdfExporter.Verify(e => e.Export(It.IsAny<IExportUserData>(), It.IsAny<IExportGoalData>()), Times.Never);
        }

        [Fact]
        public void ExportGoal_WithCaseInsensitiveFormat_ShouldWork()
        {
            // Arrange
            var goalId = 1;
            var userId = 1;
            var format = "pdf"; // lowercase

            var user = new User { Id = userId, UserName = "testuser", FirstName = "Test", LastName = "User", Email = "test@example.com", PasswordHash = "hash", PasswordSalt = "salt" };
            var goal = new Goal
            {
                Id = goalId,
                Name = "Test Goal",
                UserId = userId,
                startDate = DateTime.UtcNow,
                targetDate = DateTime.UtcNow.AddDays(30)
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            _mockGoalService.Setup(s => s.GetGoalById(goalId, userId)).Returns(goal);

            // Act
            var result = _goalExportService.ExportGoal(goalId, userId, format);

            // Assert
            result.Should().NotBeNull();
            _mockPdfExporter.Verify(e => e.Export(It.IsAny<IExportUserData>(), It.IsAny<IExportGoalData>()), Times.Once);
        }

        [Fact]
        public void ExportGoal_WithUnsupportedFormat_ShouldThrowArgumentException()
        {
            // Arrange
            var goalId = 1;
            var userId = 1;
            var format = "Word";

            var user = new User { Id = userId, UserName = "testuser", FirstName = "Test", LastName = "User", Email = "test@example.com", PasswordHash = "hash", PasswordSalt = "salt" };
            var goal = new Goal
            {
                Id = goalId,
                Name = "Test Goal",
                UserId = userId,
                startDate = DateTime.UtcNow,
                targetDate = DateTime.UtcNow.AddDays(30)
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            _mockGoalService.Setup(s => s.GetGoalById(goalId, userId)).Returns(goal);

            // Act & Assert
            var act = () => _goalExportService.ExportGoal(goalId, userId, format);
            act.Should().Throw<ArgumentException>()
                .WithMessage("Format 'Word' not supported");
        }

        [Fact]
        public void ExportGoal_WhenGoalNotFound_ShouldThrowUnauthorizedAccessException()
        {
            // Arrange
            var goalId = 999;
            var userId = 1;
            var format = "PDF";

            var user = new User { Id = userId, UserName = "testuser", FirstName = "Test", LastName = "User", Email = "test@example.com", PasswordHash = "hash", PasswordSalt = "salt" };
            _context.Users.Add(user);
            _context.SaveChanges();

            _mockGoalService.Setup(s => s.GetGoalById(goalId, userId)).Returns((Goal?)null);

            // Act & Assert
            var act = () => _goalExportService.ExportGoal(goalId, userId, format);
            act.Should().Throw<UnauthorizedAccessException>();
        }

        [Fact]
        public void ExportGoal_WhenGoalBelongsToDifferentUser_ShouldThrowUnauthorizedAccessException()
        {
            // Arrange
            var goalId = 1;
            var userId = 1;
            var format = "PDF";

            var user = new User { Id = userId, UserName = "testuser", FirstName = "Test", LastName = "User", Email = "test@example.com", PasswordHash = "hash", PasswordSalt = "salt" };
            _context.Users.Add(user);
            _context.SaveChanges();

            // Goal service returns null for unauthorized access
            _mockGoalService.Setup(s => s.GetGoalById(goalId, userId)).Returns((Goal?)null);

            // Act & Assert
            var act = () => _goalExportService.ExportGoal(goalId, userId, format);
            act.Should().Throw<UnauthorizedAccessException>();
        }

        [Fact]
        public void ExportGoal_ShouldCallGoalService()
        {
            // Arrange
            var goalId = 1;
            var userId = 1;
            var format = "PDF";

            var user = new User { Id = userId, UserName = "testuser", FirstName = "Test", LastName = "User", Email = "test@example.com", PasswordHash = "hash", PasswordSalt = "salt" };
            var goal = new Goal
            {
                Id = goalId,
                Name = "Test Goal",
                UserId = userId,
                startDate = DateTime.UtcNow,
                targetDate = DateTime.UtcNow.AddDays(30)
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            _mockGoalService.Setup(s => s.GetGoalById(goalId, userId)).Returns(goal);

            // Act
            _goalExportService.ExportGoal(goalId, userId, format);

            // Assert
            _mockGoalService.Verify(s => s.GetGoalById(goalId, userId), Times.Once);
        }

        #endregion

        #region GetAvailableFormats Tests

        [Fact]
        public void GetAvailableFormats_ShouldReturnAllExporterFormats()
        {
            // Act
            var result = _goalExportService.GetAvailableFormats();

            // Assert
            result.Should().NotBeNull();
            result.Should().Contain("PDF");
            result.Should().Contain("Excel");
            result.Should().HaveCount(2);
        }

        [Fact]
        public void GetAvailableFormats_ShouldReturnFormatNames()
        {
            // Act
            var result = _goalExportService.GetAvailableFormats().ToList();

            // Assert
            result.Should().BeEquivalentTo(new[] { "PDF", "Excel" });
        }

        [Fact]
        public void GetAvailableFormats_WithNoExporters_ShouldReturnEmptyList()
        {
            // Arrange
            var emptyExporters = new List<IExporter>();
            var serviceWithNoExporters = new GoalExportService(
                emptyExporters,
                _mockGoalService.Object,
                _mockProgressService.Object,
                _context
            );

            // Act
            var result = serviceWithNoExporters.GetAvailableFormats();

            // Assert
            result.Should().BeEmpty();
        }

        #endregion

        #region Open/Closed Principle Tests

        [Fact]
        public void ExportGoal_WithNewExporter_ShouldWorkWithoutModifyingService()
        {
            // Arrange - Add a new exporter without modifying GoalExportService
            var mockCsvExporter = new Mock<IExporter>();
            mockCsvExporter.Setup(e => e.FormatName).Returns("CSV");
            mockCsvExporter.Setup(e => e.ContentType).Returns("text/csv");
            mockCsvExporter.Setup(e => e.Export(It.IsAny<IExportUserData>(), It.IsAny<IExportGoalData>()))
                .Returns(new byte[] { 7, 8, 9 });

            var exportersWithCsv = new List<IExporter>
            {
                _mockPdfExporter.Object,
                _mockExcelExporter.Object,
                mockCsvExporter.Object
            };

            var extendedService = new GoalExportService(
                exportersWithCsv,
                _mockGoalService.Object,
                _mockProgressService.Object,
                _context
            );

            var goalId = 1;
            var userId = 1;
            var format = "CSV";

            var user = new User { Id = userId, UserName = "testuser", FirstName = "Test", LastName = "User", Email = "test@example.com", PasswordHash = "hash", PasswordSalt = "salt" };
            var goal = new Goal
            {
                Id = goalId,
                Name = "Test Goal",
                UserId = userId,
                startDate = DateTime.UtcNow,
                targetDate = DateTime.UtcNow.AddDays(30)
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            _mockGoalService.Setup(s => s.GetGoalById(goalId, userId)).Returns(goal);

            // Act
            var result = extendedService.ExportGoal(goalId, userId, format);

            // Assert
            result.Should().BeEquivalentTo(new byte[] { 7, 8, 9 });
            mockCsvExporter.Verify(e => e.Export(It.IsAny<IExportUserData>(), It.IsAny<IExportGoalData>()), Times.Once);
        }

        #endregion
    }
}