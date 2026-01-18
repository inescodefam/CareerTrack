using CareerTrack.Models;
using CareerTrack.Services;
using CareerTrack.Services.ExporterData;
using FluentAssertions;

namespace CareerTrack.Tests.UnitTests.Services
{
    public class PdfExporterTests
    {
        private readonly PdfExporter _pdfExporter;

        public PdfExporterTests()
        {
            _pdfExporter = new PdfExporter();
        }

        [Fact]
        public void PdfExporter_FormatName_ShouldBePDF()
        {
            // Act
            var formatName = _pdfExporter.FormatName;

            // Assert
            formatName.Should().Be("PDF");
        }

        [Fact]
        public void PdfExporter_ContentType_ShouldBeApplicationPdf()
        {
            // Act
            var contentType = _pdfExporter.ContentType;

            // Assert
            contentType.Should().Be("application/pdf");
        }

        [Fact]
        public void PdfExporter_Export_ShouldReturnByteArray()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                UserName = "testuser",
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                PasswordHash = "hash",
                PasswordSalt = "salt"
            };
            var goal = new Goal
            {
                Id = 1,
                Name = "Test Goal",
                Description = "Test Description",
                startDate = new DateTime(2024, 1, 1, 2, 2, 2, DateTimeKind.Utc),
                targetDate = new DateTime(2024, 12, 31, 2, 2, 2, DateTimeKind.Utc),
                UserId = 1
            };

            var exportableUser = new ExportableUser(user);
            var exportableGoal = new ExportableGoal(goal);

            // Act
            var result = _pdfExporter.Export(exportableUser, exportableGoal);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<byte[]>();
            result.Length.Should().BeGreaterThan(0);
        }

        [Fact]
        public void PdfExporter_Export_ShouldContainGoalTitle()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                UserName = "testuser",
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane@example.com",
                PasswordHash = "hash",
                PasswordSalt = "salt"
            };
            var goal = new Goal
            {
                Name = "Complete Project X",
                startDate = DateTime.UtcNow,
                targetDate = DateTime.UtcNow.AddDays(30)
            };

            var exportableUser = new ExportableUser(user);
            var exportableGoal = new ExportableGoal(goal);

            // Act
            var result = _pdfExporter.Export(exportableUser, exportableGoal);
            var content = System.Text.Encoding.UTF8.GetString(result);

            // Assert
            content.Should().Contain("Complete Project X");
        }

        [Fact]
        public void PdfExporter_Export_ShouldContainUserName()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                UserName = "testuser",
                FirstName = "Alice",
                LastName = "Johnson",
                Email = "alice@example.com",
                PasswordHash = "hash",
                PasswordSalt = "salt"
            };
            var goal = new Goal
            {
                Name = "Test Goal",
                startDate = DateTime.UtcNow,
                targetDate = DateTime.UtcNow.AddDays(30)
            };

            var exportableUser = new ExportableUser(user);
            var exportableGoal = new ExportableGoal(goal);

            // Act
            var result = _pdfExporter.Export(exportableUser, exportableGoal);
            var content = System.Text.Encoding.UTF8.GetString(result);

            // Assert
            content.Should().Contain("Alice Johnson");
        }

        [Fact]
        public void PdfExporter_Export_ShouldContainHtmlStructure()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                UserName = "testuser",
                FirstName = "Bob",
                LastName = "Brown",
                Email = "bob@example.com",
                PasswordHash = "hash",
                PasswordSalt = "salt"
            };
            var goal = new Goal
            {
                Name = "Learn Testing",
                startDate = DateTime.UtcNow,
                targetDate = DateTime.UtcNow.AddDays(30)
            };

            var exportableUser = new ExportableUser(user);
            var exportableGoal = new ExportableGoal(goal);

            // Act
            var result = _pdfExporter.Export(exportableUser, exportableGoal);
            var content = System.Text.Encoding.UTF8.GetString(result);

            // Assert
            content.Should().Contain("<html>");
            content.Should().Contain("</html>");
            content.Should().Contain("<head>");
            content.Should().Contain("<body>");
            content.Should().Contain("PDF:");
        }

        [Fact]
        public void PdfExporter_ShouldImplementIExporter()
        {
            // Assert
            _pdfExporter.Should().BeAssignableTo<IExporter>();
        }
    }

    public class ExcelExporterTests
    {
        private readonly ExcelExporter _excelExporter;

        public ExcelExporterTests()
        {
            _excelExporter = new ExcelExporter();
        }

        [Fact]
        public void ExcelExporter_FormatName_ShouldBeExcel()
        {
            // Act
            var formatName = _excelExporter.FormatName;

            // Assert
            formatName.Should().Be("Excel");
        }

        [Fact]
        public void ExcelExporter_ContentType_ShouldBeCorrect()
        {
            // Act
            var contentType = _excelExporter.ContentType;

            // Assert
            contentType.Should().Be("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        }

        [Fact]
        public void ExcelExporter_Export_WithExportData_ShouldReturnByteArray()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                UserName = "testuser",
                FirstName = "Charlie",
                LastName = "Davis",
                Email = "charlie@example.com",
                PasswordHash = "hash",
                PasswordSalt = "salt"
            };
            var goal = new Goal
            {
                Name = "Test Goal",
                startDate = new DateTime(2024, 1, 1, 2, 2, 2, DateTimeKind.Utc),
                targetDate = new DateTime(2024, 12, 31, 2, 2, 2, DateTimeKind.Utc)
            };
            var exportData = new ExportData { User = user, Goal = goal };

            // Act
            var result = ExcelExporter.Export(exportData);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<byte[]>();
            result.Length.Should().BeGreaterThan(0);
        }

        [Fact]
        public void ExcelExporter_Export_ShouldContainCsvHeaders()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                UserName = "testuser",
                FirstName = "Diana",
                LastName = "Evans",
                Email = "diana@example.com",
                PasswordHash = "hash",
                PasswordSalt = "salt"
            };
            var goal = new Goal
            {
                Name = "Test Goal",
                startDate = DateTime.UtcNow,
                targetDate = DateTime.UtcNow.AddDays(30)
            };
            var exportData = new ExportData { User = user, Goal = goal };

            // Act
            var result = ExcelExporter.Export(exportData);
            var content = System.Text.Encoding.UTF8.GetString(result);

            // Assert
            content.Should().Contain("Goal Name");
            content.Should().Contain("Owner");
            content.Should().Contain("Start Date");
            content.Should().Contain("Target Date");
            content.Should().Contain("Progress");
        }

        [Fact]
        public void ExcelExporter_Export_ShouldContainGoalData()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                UserName = "testuser",
                FirstName = "Eve",
                LastName = "Foster",
                Email = "eve@example.com",
                PasswordHash = "hash",
                PasswordSalt = "salt"
            };
            var goal = new Goal
            {
                Name = "Complete Testing",
                startDate = new DateTime(2024, 6, 1, 2, 2, 2, DateTimeKind.Utc),
                targetDate = new DateTime(2024, 12, 31, 2, 2, 2, DateTimeKind.Utc)
            };
            var exportData = new ExportData { User = user, Goal = goal };

            // Act
            var result = ExcelExporter.Export(exportData);
            var content = System.Text.Encoding.UTF8.GetString(result);

            // Assert
            content.Should().Contain("Complete Testing");
            content.Should().Contain("Eve Foster");
            content.Should().Contain("2024-06-01");
            content.Should().Contain("2024-12-31");
        }

        [Fact]
        public void ExcelExporter_ShouldImplementIExporter()
        {
            // Assert
            _excelExporter.Should().BeAssignableTo<IExporter>();
        }

        [Fact]
        public void ExcelExporter_Export_WithIExportData_ShouldThrowNotImplementedException()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                UserName = "testuser",
                FirstName = "Test",
                LastName = "User",
                Email = "test@example.com",
                PasswordHash = "hash",
                PasswordSalt = "salt"
            };
            var goal = new Goal
            {
                Name = "Test",
                startDate = DateTime.UtcNow,
                targetDate = DateTime.UtcNow.AddDays(30)
            };
            var exportableUser = new ExportableUser(user);
            var exportableGoal = new ExportableGoal(goal);

            // Act & Assert
            var act = () => _excelExporter.Export(exportableUser, exportableGoal);
            act.Should().Throw<NotImplementedException>();
        }
    }

    public class ExportDataTests
    {
        [Fact]
        public void ExportData_ShouldStoreUserAndGoal()
        {
            // Arrange & Act
            var user = new User
            {
                Id = 1,
                UserName = "testuser",
                FirstName = "Frank",
                LastName = "Garcia",
                Email = "frank@example.com",
                PasswordHash = "hash",
                PasswordSalt = "salt"
            };
            var goal = new Goal
            {
                Id = 1,
                Name = "Test Goal",
                startDate = DateTime.UtcNow,
                targetDate = DateTime.UtcNow.AddDays(30)
            };
            var exportData = new ExportData
            {
                User = user,
                Goal = goal
            };

            // Assert
            exportData.User.Should().Be(user);
            exportData.Goal.Should().Be(goal);
        }

        [Fact]
        public void ExportData_ShouldAllowDifferentUsers()
        {
            // Arrange
            var user1 = new User { Id = 1, UserName = "user1", FirstName = "User", LastName = "One", Email = "user1@example.com", PasswordHash = "hash", PasswordSalt = "salt" };
            var user2 = new User { Id = 2, UserName = "user2", FirstName = "User", LastName = "Two", Email = "user2@example.com", PasswordHash = "hash", PasswordSalt = "salt" };
            var goal = new Goal { Name = "Goal", startDate = DateTime.UtcNow, targetDate = DateTime.UtcNow.AddDays(30) };

            // Act
            var exportData1 = new ExportData { User = user1, Goal = goal };
            var exportData2 = new ExportData { User = user2, Goal = goal };

            // Assert
            exportData1.User.Should().Be(user1);
            exportData2.User.Should().Be(user2);
            exportData1.User.Should().NotBe(exportData2.User);
        }
    }
}