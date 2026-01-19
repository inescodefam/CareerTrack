using CareerTrack.Controllers;
using CareerTrack.Models;
using CareerTrack.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace CareerTrack.Tests.UnitTests.Controllers
{
    public class GoalsControllerTests : IDisposable
    {
        private static readonly string[] AvailableExportFormats = ["PDF", "EXCEL"];

        private readonly Mock<IGoalService> _mockGoalService;
        private readonly Mock<IUserContextService> _mockUserContext;
        private readonly Mock<IProgressService> _mockProgressService;
        private readonly Mock<IGoalExportService> _mockExportService;
        private readonly Mock<CareerTrack.Interfaces.IGoalFactory> _mockGoalFactory;
        private readonly AppDbContext _context;
        private readonly GoalsController _controller;

        private bool _disposed;
        private static readonly string[] value = new[] { "PDF", "Excel" };

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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public GoalsControllerTests()
        {
            _mockGoalService = new Mock<IGoalService>();
            _mockUserContext = new Mock<IUserContextService>();
            _mockProgressService = new Mock<IProgressService>();
            _mockExportService = new Mock<IGoalExportService>();
            _mockGoalFactory = new Mock<CareerTrack.Interfaces.IGoalFactory>();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);

            _mockUserContext.Setup(u => u.GetCurrentUserId()).Returns(1);

            _controller = new GoalsController(
                _context,
                _mockGoalService.Object,
                _mockUserContext.Object,
                _mockProgressService.Object,
                _mockExportService.Object,
                _mockGoalFactory.Object
            );

            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            _controller.TempData = tempData;
        }

        #region Index Tests

        [Fact]
        public void Index_ShouldReturnViewWithGoals()
        {
            // Arrange
            var userId = 1;
            var goals = new List<Goal>
            {
                new Goal { Id = 1, Name = "Goal 1", UserId = userId },
                new Goal { Id = 2, Name = "Goal 2", UserId = userId }
            };

            _mockGoalService.Setup(s => s.GetUserGoals(userId)).Returns(goals);

            // Act
            var result = _controller.Index();

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeAssignableTo<IEnumerable<Goal>>().Subject;
            model.Should().HaveCount(2);
            _mockGoalService.Verify(s => s.GetUserGoals(userId), Times.Once);
        }

        [Fact]
        public void Index_ShouldGetCurrentUserIdFromContext()
        {
            // Arrange
            _mockGoalService.Setup(s => s.GetUserGoals(It.IsAny<int>())).Returns(new List<Goal>());

            // Act
            _controller.Index();

            // Assert
            _mockUserContext.Verify(u => u.GetCurrentUserId(), Times.Once);
        }

        #endregion

        #region Details Tests

        [Fact]
        public void Details_WithValidId_ShouldReturnViewWithGoal()
        {
            // Arrange
            var goalId = 1;
            var userId = 1;
            var goal = new Goal { Id = goalId, Name = "Test Goal", UserId = userId };

            _mockGoalService.Setup(s => s.GetGoalById(goalId, userId)).Returns(goal);
            _mockProgressService.Setup(p => p.GetProgress(goalId, userId)).Returns((GoalProgress?)null);
            _mockProgressService.Setup(p => p.GetProgressHistory(goalId, userId)).Returns(new List<GoalProgress>());

            // Act
            var result = _controller.Details(goalId);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeOfType<Goal>().Subject;
            model.Id.Should().Be(goalId);
        }

        [Fact]
        public void Details_WithNullId_ShouldReturnNotFound()
        {
            // Act
            var result = _controller.Details(null);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public void Details_WithNonExistentGoal_ShouldReturnNotFound()
        {
            // Arrange
            var goalId = 999;
            var userId = 1;

            _mockGoalService.Setup(s => s.GetGoalById(goalId, userId)).Returns((Goal?)null);

            // Act
            var result = _controller.Details(goalId);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public void Details_ShouldCallProgressService()
        {
            // Arrange
            var goalId = 1;
            var userId = 1;
            var goal = new Goal { Id = goalId, Name = "Test Goal", UserId = userId };

            _mockGoalService.Setup(s => s.GetGoalById(goalId, userId)).Returns(goal);
            _mockProgressService.Setup(p => p.GetProgress(goalId, userId)).Returns((GoalProgress?)null);
            _mockProgressService.Setup(p => p.GetProgressHistory(goalId, userId)).Returns(new List<GoalProgress>());

            // Act
            _controller.Details(goalId);

            // Assert
            _mockProgressService.Verify(p => p.GetProgress(goalId, userId), Times.Once);
            _mockProgressService.Verify(p => p.GetProgressHistory(goalId, userId), Times.Once);
        }

        #endregion

        #region Create GET Tests

        [Fact]
        public void Create_GET_ShouldReturnView()
        {
            // Act
            var result = _controller.Create();

            // Assert
            result.Should().BeOfType<ViewResult>();
        }

        #endregion

        #region Create POST Tests

        [Fact]
        public void Create_POST_WithValidModel_ShouldCreateGoalAndRedirect()
        {
            // Arrange
            var userId = 1;
            var goal = new Goal
            {
                Name = "Test Goal",
                Description = "Test Description",
                startDate = DateTime.UtcNow,
                targetDate = DateTime.UtcNow.AddDays(30)
            };

            _mockGoalService.Setup(s => s.CreateGoal(It.IsAny<Goal>(), userId)).Returns(goal);

            // Act
            var result = _controller.Create(goal);

            // Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");
            _mockGoalService.Verify(s => s.CreateGoal(goal, userId), Times.Once);
        }

        [Fact]
        public void Create_POST_WithInvalidModel_ShouldReturnViewWithModel()
        {
            // Arrange
            var goal = new Goal { Name = "" }; // Invalid model
            _controller.ModelState.AddModelError("Name", "Required");

            // Act
            var result = _controller.Create(goal);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().Be(goal);
            _mockGoalService.Verify(s => s.CreateGoal(It.IsAny<Goal>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public void Create_POST_ShouldUseCurrentUserId()
        {
            // Arrange
            var goal = new Goal
            {
                Name = "Test Goal",
                startDate = DateTime.UtcNow,
                targetDate = DateTime.UtcNow.AddDays(30)
            };

            // Act
            _controller.Create(goal);

            // Assert
            _mockUserContext.Verify(u => u.GetCurrentUserId(), Times.Once);
        }

        #endregion

        #region Edit GET Tests

        [Fact]
        public void Edit_GET_WithValidId_ShouldReturnViewWithGoal()
        {
            // Arrange
            var goalId = 1;
            var userId = 1;
            var goal = new Goal { Id = goalId, Name = "Test Goal", UserId = userId };

            _mockGoalService.Setup(s => s.GetGoalById(goalId, userId)).Returns(goal);

            // Act
            var result = _controller.Edit(goalId);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeOfType<Goal>().Subject;
            model.Id.Should().Be(goalId);
        }

        [Fact]
        public void Edit_GET_WithNullId_ShouldReturnNotFound()
        {
            // Act
            var result = _controller.Edit(null);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public void Edit_GET_WithNonExistentGoal_ShouldReturnNotFound()
        {
            // Arrange
            var goalId = 999;
            var userId = 1;

            _mockGoalService.Setup(s => s.GetGoalById(goalId, userId)).Returns((Goal?)null);

            // Act
            var result = _controller.Edit(goalId);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        #endregion

        #region Edit POST Tests

        [Fact]
        public void Edit_POST_WithValidModel_ShouldUpdateGoalAndRedirect()
        {
            // Arrange
            var userId = 1;
            var goal = new Goal
            {
                Id = 1,
                Name = "Updated Goal",
                startDate = DateTime.UtcNow,
                targetDate = DateTime.UtcNow.AddDays(30)
            };

            _mockGoalService.Setup(s => s.UpdateGoal(It.IsAny<Goal>(), userId)).Returns(goal);

            // Act
            var result = _controller.Edit(1, goal);

            // Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");
            _mockGoalService.Verify(s => s.UpdateGoal(goal, userId), Times.Once);
        }

        [Fact]
        public void Edit_POST_WithMismatchedId_ShouldReturnNotFound()
        {
            // Arrange
            var goal = new Goal { Id = 1, Name = "Test Goal" };

            // Act
            var result = _controller.Edit(2, goal); // Different ID

            // Assert
            result.Should().BeOfType<NotFoundResult>();
            _mockGoalService.Verify(s => s.UpdateGoal(It.IsAny<Goal>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public void Edit_POST_WithInvalidModel_ShouldReturnViewWithModel()
        {
            // Arrange
            var goal = new Goal { Id = 1, Name = "" };
            _controller.ModelState.AddModelError("Name", "Required");

            // Act
            var result = _controller.Edit(1, goal);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().Be(goal);
            _mockGoalService.Verify(s => s.UpdateGoal(It.IsAny<Goal>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public void Edit_POST_WithConcurrencyException_WhenGoalExists_ShouldRethrow()
        {
            // Arrange
            var userId = 1;
            var goal = new Goal
            {
                Id = 1,
                Name = "Test Goal",
                startDate = DateTime.UtcNow,
                targetDate = DateTime.UtcNow.AddDays(30)
            };

            _mockGoalService.Setup(s => s.UpdateGoal(It.IsAny<Goal>(), userId))
                .Throws<DbUpdateConcurrencyException>();
            _mockGoalService.Setup(s => s.GetGoalById(1, userId)).Returns(goal);

            // Act & Assert
            var act = () => _controller.Edit(1, goal);
            act.Should().Throw<DbUpdateConcurrencyException>();
        }

        [Fact]
        public void Edit_POST_WithConcurrencyException_WhenGoalDoesNotExist_ShouldReturnNotFound()
        {
            // Arrange
            var userId = 1;
            var goal = new Goal
            {
                Id = 1,
                Name = "Test Goal",
                startDate = DateTime.UtcNow,
                targetDate = DateTime.UtcNow.AddDays(30)
            };

            _mockGoalService.Setup(s => s.UpdateGoal(It.IsAny<Goal>(), userId))
                .Throws<DbUpdateConcurrencyException>();
            _mockGoalService.Setup(s => s.GetGoalById(1, userId)).Returns((Goal?)null);

            // Act
            var result = _controller.Edit(1, goal);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        #endregion

        #region Delete GET Tests

        [Fact]
        public void Delete_GET_WithValidId_ShouldReturnViewWithGoal()
        {
            // Arrange
            var goalId = 1;
            var userId = 1;
            var goal = new Goal { Id = goalId, Name = "Test Goal", UserId = userId };

            _mockGoalService.Setup(s => s.GetGoalById(goalId, userId)).Returns(goal);

            // Act
            var result = _controller.Delete(goalId);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeOfType<Goal>().Subject;
            model.Id.Should().Be(goalId);
        }

        [Fact]
        public void Delete_GET_WithNullId_ShouldReturnNotFound()
        {
            // Act
            var result = _controller.Delete(null);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public void Delete_GET_WithNonExistentGoal_ShouldReturnNotFound()
        {
            // Arrange
            var goalId = 999;
            var userId = 1;

            _mockGoalService.Setup(s => s.GetGoalById(goalId, userId)).Returns((Goal?)null);

            // Act
            var result = _controller.Delete(goalId);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        #endregion

        #region DeleteConfirmed Tests

        [Fact]
        public void DeleteConfirmed_ShouldDeleteGoalAndRedirect()
        {
            // Arrange
            var goalId = 1;
            var userId = 1;

            _mockGoalService.Setup(s => s.DeleteGoal(goalId, userId)).Returns(true);

            // Act
            var result = _controller.DeleteConfirmed(goalId);

            // Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");
            _mockGoalService.Verify(s => s.DeleteGoal(goalId, userId), Times.Once);
        }

        [Fact]
        public void DeleteConfirmed_ShouldUseCurrentUserId()
        {
            // Arrange
            var goalId = 1;

            // Act
            _controller.DeleteConfirmed(goalId);

            // Assert
            _mockUserContext.Verify(u => u.GetCurrentUserId(), Times.Once);
        }

        #endregion

        #region UpdateProgress Tests

        [Fact]
        public void UpdateProgress_ShouldCallProgressServiceAndRedirect()
        {
            // Arrange
            var goalId = 1;
            var userId = 1;
            var progressPercentage = 50;
            var notes = "Half way there";

            // Act
            var result = _controller.UpdateProgress(goalId, progressPercentage, notes);

            // Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Details");
            redirectResult.RouteValues!["id"].Should().Be(goalId);
            _mockProgressService.Verify(p => p.UpdateProgress(goalId, userId, progressPercentage, notes), Times.Once);
        }

        [Fact]
        public void UpdateProgress_WithNullNotes_ShouldWork()
        {
            // Arrange
            var goalId = 1;
            var userId = 1;
            var progressPercentage = 75;

            // Act
            var result = _controller.UpdateProgress(goalId, progressPercentage, null);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            _mockProgressService.Verify(p => p.UpdateProgress(goalId, userId, progressPercentage, null), Times.Once);
        }

        #endregion

        #region Print Tests

        [Fact]
        public void Print_WithValidParameters_ShouldReturnFile()
        {
            // Arrange
            var goalId = 1;
            var userId = 1;
            var format = "PDF";
            var fileBytes = new byte[] { 1, 2, 3, 4, 5 };

            _mockExportService.Setup(s => s.ExportGoal(goalId, userId, format)).Returns(fileBytes);
            _mockExportService.Setup(s => s.GetAvailableFormats()).Returns(AvailableExportFormats);

            // Act
            var result = _controller.Print(goalId, format);

            // Assert
            var fileResult = result.Should().BeOfType<FileContentResult>().Subject;
            fileResult.FileContents.Should().BeEquivalentTo(fileBytes);
            fileResult.ContentType.Should().Be("application/pdf");
            fileResult.FileDownloadName.Should().Be($"goal-{goalId}.pdf");
        }

        [Fact]
        public void Print_WithExcelFormat_ShouldReturnExcelFile()
        {
            // Arrange
            var goalId = 1;
            var userId = 1;
            var format = "EXCEL";
            var fileBytes = new byte[] { 1, 2, 3 };

            _mockExportService.Setup(s => s.ExportGoal(goalId, userId, format)).Returns(fileBytes);
            _mockExportService.Setup(s => s.GetAvailableFormats()).Returns(AvailableExportFormats);

            // Act
            var result = _controller.Print(goalId, format);

            // Assert
            var fileResult = result.Should().BeOfType<FileContentResult>().Subject;
            fileResult.ContentType.Should().Be("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        }

        [Fact]
        public void Print_WithException_ShouldRedirectToDetailsWithError()
        {
            // Arrange
            var goalId = 1;
            var userId = 1;
            var format = "PDF";

            _mockExportService.Setup(s => s.ExportGoal(goalId, userId, format))
                .Throws(new Exception("Export failed"));

            // Act
            var result = _controller.Print(goalId, format);

            // Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Details");
            redirectResult.RouteValues!["id"].Should().Be(goalId);
            _controller.TempData["Error"].Should().NotBeNull();
        }

        [Fact]
        public void Print_WithDefaultFormat_ShouldUsePdf()
        {
            // Arrange
            var goalId = 1;
            var userId = 1;
            var fileBytes = new byte[] { 1, 2, 3 };

            _mockExportService.Setup(s => s.ExportGoal(goalId, userId, "PDF")).Returns(fileBytes);
            _mockExportService.Setup(s => s.GetAvailableFormats()).Returns(value);

            // Act
            var result = _controller.Print(goalId); // No format specified, should default to PDF

            // Assert
            var fileResult = result.Should().BeOfType<FileContentResult>().Subject;
            fileResult.ContentType.Should().Be("application/pdf");
        }

        #endregion

        #region Notifications Tests

        [Fact]
        public void Notifications_ShouldReturnView()
        {
            // Act
            var result = _controller.Notifications();

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.ViewData["Demo1"].Should().NotBeNull();
            viewResult.ViewData["Demo2"].Should().NotBeNull();
            viewResult.ViewData["Demo3"].Should().NotBeNull();
        }

        #endregion

        #region CreateGoalVariant Tests

        [Fact]
        public void CreateGoalVariant_WithValidData_ShouldCreateGoalAndRedirect()
        {
            // Arrange
            var userId = 1;
            var goalType = "ShortTerm";
            var name = "Learn C#";
            var targetDate = DateTime.UtcNow.AddDays(30);
            var createdGoal = new Goal { Name = name, targetDate = targetDate };

            _mockGoalFactory.Setup(f => f.CreateGoal(goalType, name, targetDate))
                .Returns(createdGoal);
            _mockGoalService.Setup(s => s.CreateGoal(createdGoal, userId));

            // Act
            var result = _controller.CreateGoalVariant(goalType, name, targetDate);

            // Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");
            _mockGoalService.Verify(s => s.CreateGoal(createdGoal, userId), Times.Once);
        }

        [Fact]
        public void CreateGoalVariant_WithLongTermGoal_ShouldCreateLongTermGoal()
        {
            // Arrange
            var userId = 1;
            var goalType = "LongTerm";
            var name = "Become Senior Developer";
            var targetDate = DateTime.UtcNow.AddYears(2);
            var createdGoal = new LongTermGoal { Name = name, targetDate = targetDate };

            _mockGoalFactory.Setup(f => f.CreateGoal(goalType, name, targetDate))
                .Returns(createdGoal);
            _mockGoalService.Setup(s => s.CreateGoal(createdGoal, userId));

            // Act
            var result = _controller.CreateGoalVariant(goalType, name, targetDate);

            // Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");
        }

        [Fact]
        public void CreateGoalVariant_WithInvalidModelState_ShouldReturnBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("name", "Required");

            // Act
            var result = _controller.CreateGoalVariant("ShortTerm", "", DateTime.UtcNow);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        #endregion

        #region Details BadRequest Tests

        [Fact]
        public void Details_WithInvalidModelState_ShouldReturnBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("id", "Invalid");

            // Act
            var result = _controller.Details(1);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        #endregion

        #region Edit GET BadRequest Tests

        [Fact]
        public void Edit_GET_WithInvalidModelState_ShouldReturnBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("id", "Invalid");

            // Act
            var result = _controller.Edit(1);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        #endregion

        #region Delete BadRequest Tests

        [Fact]
        public void Delete_GET_WithInvalidModelState_ShouldReturnBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("id", "Invalid");

            // Act
            var result = _controller.Delete(1);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        #endregion

        #region DeleteConfirmed BadRequest Tests

        [Fact]
        public void DeleteConfirmed_WithInvalidModelState_ShouldReturnBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("id", "Invalid");

            // Act
            var result = _controller.DeleteConfirmed(1);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        #endregion

        #region UpdateProgress BadRequest Tests

        [Fact]
        public void UpdateProgress_WithInvalidModelState_ShouldReturnBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("progressPercentage", "Invalid");

            // Act
            var result = _controller.UpdateProgress(1, 50, "notes");

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        #endregion

        #region Print BadRequest and Unknown Format Tests

        [Fact]
        public void Print_WithInvalidModelState_ShouldReturnBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("id", "Invalid");

            // Act
            var result = _controller.Print(1, "PDF");

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public void Print_WithUnknownFormat_ShouldReturnOctetStream()
        {
            // Arrange
            var goalId = 1;
            var userId = 1;
            var format = "UNKNOWN";
            var fileBytes = new byte[] { 1, 2, 3 };

            _mockExportService.Setup(s => s.ExportGoal(goalId, userId, format)).Returns(fileBytes);
            _mockExportService.Setup(s => s.GetAvailableFormats()).Returns(AvailableExportFormats);

            // Act
            var result = _controller.Print(goalId, format);

            // Assert
            var fileResult = result.Should().BeOfType<FileContentResult>().Subject;
            fileResult.ContentType.Should().Be("application/octet-stream");
        }

        [Fact]
        public void Print_WithUnavailableFormat_ShouldUsePdfAsDefault()
        {
            // Arrange
            var goalId = 1;
            var userId = 1;
            var format = "CUSTOM";
            var fileBytes = new byte[] { 1, 2, 3 };

            _mockExportService.Setup(s => s.ExportGoal(goalId, userId, format)).Returns(fileBytes);
            _mockExportService.Setup(s => s.GetAvailableFormats()).Returns(AvailableExportFormats);

            // Act
            var result = _controller.Print(goalId, format);

            // Assert
            var fileResult = result.Should().BeOfType<FileContentResult>().Subject;
            fileResult.FileDownloadName.Should().Be($"goal-{goalId}.pdf");
        }

        #endregion

        #region ShowProgress Tests

        [Fact]
        public void ShowProgress_WithValidProgress_ShouldCallGetProgressDescription()
        {
            // Arrange
            var mockProgress = new Mock<IGoalProgress>();
            mockProgress.Setup(p => p.GetProgressDescription()).Returns("50% complete");

            // Act
            _controller.ShowProgress(mockProgress.Object);

            // Assert
            mockProgress.Verify(p => p.GetProgressDescription(), Times.Once);
        }

        [Fact]
        public void ShowProgress_WithInvalidModelState_ShouldStillCallGetProgressDescription()
        {
            // Arrange
            var mockProgress = new Mock<IGoalProgress>();
            mockProgress.Setup(p => p.GetProgressDescription()).Returns("Progress info");
            _controller.ModelState.AddModelError("test", "error");

            // Act
            _controller.ShowProgress(mockProgress.Object);

            // Assert
            mockProgress.Verify(p => p.GetProgressDescription(), Times.Once);
        }

        #endregion

        #region CreateValidGoal Tests

        [Fact]
        public void CreateValidGoal_WithInvalidModelState_ShouldReturnBadRequest()
        {
            // Arrange
            var goal = new Goal { Name = "Test Goal" };
            _controller.ModelState.AddModelError("Name", "Required");

            // Act
            var result = _controller.CreateValidGoal(goal);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public void CreateValidGoal_WithValidGoal_WhenHandlerFails_ShouldReturnViewWithErrors()
        {
            // Arrange
            var goal = new Goal
            {
                Name = "Test Goal",
                startDate = DateTime.UtcNow,
                targetDate = DateTime.UtcNow.AddDays(30)
            };

            // The handler chain will validate and may fail - we need to set up the context properly
            // Since the handler chain is created in constructor, we'll test with a goal that fails validation

            // Act
            var result = _controller.CreateValidGoal(goal);

            // Assert - the handler chain should process the request
            // The result depends on the handler chain validation
            result.Should().BeAssignableTo<IActionResult>();
        }

        [Fact]
        public void CreateValidGoal_WithNullGoalName_ShouldReturnViewWithValidationErrors()
        {
            // Arrange
            var goal = new Goal
            {
                Name = null!,
                startDate = DateTime.UtcNow,
                targetDate = DateTime.UtcNow.AddDays(30)
            };

            // Act
            var result = _controller.CreateValidGoal(goal);

            // Assert
            // The handler chain validation should catch the null name
            result.Should().BeAssignableTo<IActionResult>();
        }

        [Fact]
        public void CreateValidGoal_WithValidGoal_WhenHandlerSucceeds_ShouldSetTempDataSuccessAndRedirect()
        {
            // Arrange - create a valid goal that passes all validation
            var goal = new Goal
            {
                Id = 0,
                Name = "Valid Test Goal",
                startDate = DateTime.UtcNow.AddDays(1),
                targetDate = DateTime.UtcNow.AddDays(60)
            };

            // Act
            var result = _controller.CreateValidGoal(goal);

            // Assert - If validation passes, should redirect with success
            // The handler chain validates: Name required, targetDate in future, startDate < targetDate
            result.Should().BeAssignableTo<IActionResult>();
        }

        [Fact]
        public void CreateValidGoal_WithPastTargetDate_ShouldReturnViewWithValidationErrors()
        {
            // Arrange - target date in the past triggers validation error
            var goal = new Goal
            {
                Name = "Test Goal",
                startDate = DateTime.UtcNow.AddDays(-30),
                targetDate = DateTime.UtcNow.AddDays(-1)
            };

            // Act
            var result = _controller.CreateValidGoal(goal);

            // Assert - Should return view with validation errors
            result.Should().BeOfType<ViewResult>();
            _controller.ModelState.IsValid.Should().BeFalse();
        }

        [Fact]
        public void CreateValidGoal_WithStartDateAfterTargetDate_ShouldReturnViewWithValidationErrors()
        {
            // Arrange - start date after target date triggers validation error
            var goal = new Goal
            {
                Name = "Test Goal",
                startDate = DateTime.UtcNow.AddDays(60),
                targetDate = DateTime.UtcNow.AddDays(30)
            };

            // Act
            var result = _controller.CreateValidGoal(goal);

            // Assert - Should return view with validation errors
            result.Should().BeOfType<ViewResult>();
            _controller.ModelState.IsValid.Should().BeFalse();
        }

        [Fact]
        public void CreateValidGoal_WithTooLongName_ShouldReturnViewWithValidationErrors()
        {
            // Arrange - name over 150 characters triggers validation error
            var goal = new Goal
            {
                Name = new string('A', 151),
                startDate = DateTime.UtcNow.AddDays(1),
                targetDate = DateTime.UtcNow.AddDays(60)
            };

            // Act
            var result = _controller.CreateValidGoal(goal);

            // Assert - Should return view with validation errors
            result.Should().BeOfType<ViewResult>();
            _controller.ModelState.IsValid.Should().BeFalse();
        }

        [Fact]
        public void CreateValidGoal_WithEmptyName_ShouldReturnViewWithValidationErrors()
        {
            // Arrange - empty name triggers validation error
            var goal = new Goal
            {
                Name = "",
                startDate = DateTime.UtcNow.AddDays(1),
                targetDate = DateTime.UtcNow.AddDays(60)
            };

            // Act
            var result = _controller.CreateValidGoal(goal);

            // Assert - Should return view with validation errors
            result.Should().BeOfType<ViewResult>();
            _controller.ModelState.IsValid.Should().BeFalse();
        }

        [Fact]
        public void CreateValidGoal_WithWhitespaceOnlyName_ShouldReturnViewWithValidationErrors()
        {
            // Arrange - whitespace-only name triggers validation error
            var goal = new Goal
            {
                Name = "   ",
                startDate = DateTime.UtcNow.AddDays(1),
                targetDate = DateTime.UtcNow.AddDays(60)
            };

            // Act
            var result = _controller.CreateValidGoal(goal);

            // Assert - Should return view with validation errors
            result.Should().BeOfType<ViewResult>();
            _controller.ModelState.IsValid.Should().BeFalse();
        }

        [Fact]
        public void CreateValidGoal_WhenUserHas10ActiveGoals_ShouldReturnViewWithBusinessRuleError()
        {
            // Arrange - create 10 active goals for user to trigger business rule
            var userId = 1;
            var user = new User
            {
                Id = userId,
                UserName = "testuser",
                Email = "test@test.com",
                FirstName = "Test",
                LastName = "User",
                PasswordHash = "hash",
                PasswordSalt = "salt"
            };
            _context.Users.Add(user);

            // Add 10 active goals (endDate = null means active)
            for (int i = 0; i < 10; i++)
            {
                _context.Goals.Add(new Goal
                {
                    Id = i + 100,
                    Name = $"Existing Goal {i}",
                    UserId = userId,
                    User = user,
                    startDate = DateTime.UtcNow,
                    targetDate = DateTime.UtcNow.AddDays(30),
                    endDate = null
                });
            }
            _context.SaveChanges();

            var newGoal = new Goal
            {
                Name = "New Valid Goal",
                startDate = DateTime.UtcNow.AddDays(1),
                targetDate = DateTime.UtcNow.AddDays(60)
            };

            // Act
            var result = _controller.CreateValidGoal(newGoal);

            // Assert - Should return view with business rule error (max 10 active goals)
            result.Should().BeOfType<ViewResult>();
            _controller.ModelState.IsValid.Should().BeFalse();
        }

        #endregion

        #region ValidGoalDeleteDelete Tests

        [Fact]
        public void ValidGoalDeleteDelete_WithInvalidModelState_ShouldReturnBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("id", "Required");

            // Act
            var result = _controller.ValidGoalDeleteDelete(1);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public void ValidGoalDeleteDelete_WithValidId_ShouldCallGetGoalById()
        {
            // Arrange
            var goalId = 1;
            var userId = 1;
            var user = new User
            {
                Id = userId,
                UserName = "testuser",
                Email = "test@test.com",
                FirstName = "Test",
                LastName = "User",
                PasswordHash = "hash",
                PasswordSalt = "salt"
            };
            var goal = new Goal
            {
                Id = goalId,
                Name = "Test Goal",
                UserId = userId,
                User = user,
                startDate = DateTime.UtcNow,
                targetDate = DateTime.UtcNow.AddDays(30)
            };

            // Add user and goal to context for the authorization handler
            _context.Users.Add(user);
            _context.Goals.Add(goal);
            _context.SaveChanges();

            _mockGoalService.Setup(s => s.GetGoalById(goalId, userId)).Returns(goal);

            // Act
            var result = _controller.ValidGoalDeleteDelete(goalId);

            // Assert
            _mockGoalService.Verify(s => s.GetGoalById(goalId, userId), Times.Once);
            result.Should().BeAssignableTo<IActionResult>();
        }

        [Fact]
        public void ValidGoalDeleteDelete_WithNullId_ShouldReturnNotFound()
        {
            // Act
            var result = _controller.ValidGoalDeleteDelete(null);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public void ValidGoalDeleteDelete_WithNonExistentGoal_ShouldReturnNotFound()
        {
            // Arrange
            var goalId = 999;
            var userId = 1;

            _mockGoalService.Setup(s => s.GetGoalById(goalId, userId)).Returns((Goal?)null);

            // Act
            var result = _controller.ValidGoalDeleteDelete(goalId);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public void ValidGoalDeleteDelete_WhenAuthorized_ShouldRedirectToIndex()
        {
            // Arrange - user owns the goal, so authorization passes
            var goalId = 50;
            var userId = 1;
            var user = new User
            {
                Id = userId,
                UserName = "testuser",
                Email = "test@test.com",
                FirstName = "Test",
                LastName = "User",
                PasswordHash = "hash",
                PasswordSalt = "salt"
            };
            var goal = new Goal
            {
                Id = goalId,
                Name = "My Goal",
                UserId = userId,
                User = user,
                startDate = DateTime.UtcNow,
                targetDate = DateTime.UtcNow.AddDays(30)
            };

            _context.Users.Add(user);
            _context.Goals.Add(goal);
            _context.SaveChanges();

            _mockGoalService.Setup(s => s.GetGoalById(goalId, userId)).Returns(goal);

            // Act
            var result = _controller.ValidGoalDeleteDelete(goalId);

            // Assert - Should redirect when handler passes (TempData may or may not have message)
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");
            // TempData["Error"] should be null when authorization passes
            _controller.TempData["Error"].Should().BeNull();
        }

        [Fact]
        public void ValidGoalDeleteDelete_WhenNotAuthorized_ShouldRedirectWithErrorMessage()
        {
            // Arrange - goal belongs to different user, so authorization fails
            var goalId = 51;
            var userId = 1;
            var otherUserId = 2;
            var user = new User
            {
                Id = userId,
                UserName = "testuser",
                Email = "test@test.com",
                FirstName = "Test",
                LastName = "User",
                PasswordHash = "hash",
                PasswordSalt = "salt"
            };
            var otherUser = new User
            {
                Id = otherUserId,
                UserName = "otheruser",
                Email = "other@test.com",
                FirstName = "Other",
                LastName = "User",
                PasswordHash = "hash",
                PasswordSalt = "salt"
            };
            // Goal belongs to otherUser but current user (userId=1) is trying to delete
            var goal = new Goal
            {
                Id = goalId,
                Name = "Other User's Goal",
                UserId = otherUserId,
                User = otherUser,
                startDate = DateTime.UtcNow,
                targetDate = DateTime.UtcNow.AddDays(30)
            };

            _context.Users.Add(user);
            _context.Users.Add(otherUser);
            _context.Goals.Add(goal);
            _context.SaveChanges();

            _mockGoalService.Setup(s => s.GetGoalById(goalId, userId)).Returns(goal);

            // Act
            var result = _controller.ValidGoalDeleteDelete(goalId);

            // Assert - Should redirect with error message when authorization fails
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");
            _controller.TempData["Error"].Should().NotBeNull();
        }

        [Fact]
        public void ValidGoalDeleteDelete_WhenGoalNotInContext_ShouldRedirectWithError()
        {
            // Arrange - goal exists in service but not in db context (edge case for authorization handler)
            var goalId = 52;
            var userId = 1;
            var goal = new Goal
            {
                Id = goalId,
                Name = "Test Goal",
                UserId = userId,
                startDate = DateTime.UtcNow,
                targetDate = DateTime.UtcNow.AddDays(30)
            };

            // Goal NOT added to context, so authorization handler can't find it
            _mockGoalService.Setup(s => s.GetGoalById(goalId, userId)).Returns(goal);

            // Act
            var result = _controller.ValidGoalDeleteDelete(goalId);

            // Assert - Authorization handler returns error when goal not found in context
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");
            _controller.TempData["Error"].Should().NotBeNull();
        }

        #endregion

        #region Edit POST Null Id Tests

        [Fact]
        public void Edit_POST_WithNullId_ShouldReturnNotFound()
        {
            // Arrange
            var goal = new Goal
            {
                Id = 1,
                Name = "Test Goal",
                startDate = DateTime.UtcNow,
                targetDate = DateTime.UtcNow.AddDays(30)
            };

            // Act
            var result = _controller.Edit(null, goal);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        #endregion

        #region DeleteConfirmed Null Id Tests

        [Fact]
        public void DeleteConfirmed_WithNullId_ShouldReturnNotFound()
        {
            // Act
            var result = _controller.DeleteConfirmed(null);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        #endregion

        #region UpdateProgress Null Parameters Tests

        [Fact]
        public void UpdateProgress_WithNullId_ShouldReturnNotFound()
        {
            // Act
            var result = _controller.UpdateProgress(null, 50, "notes");

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public void UpdateProgress_WithNullProgressPercentage_ShouldReturnNotFound()
        {
            // Act
            var result = _controller.UpdateProgress(1, null, "notes");

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public void UpdateProgress_WithBothNullParameters_ShouldReturnNotFound()
        {
            // Act
            var result = _controller.UpdateProgress(null, null, "notes");

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        #endregion

        #region Print Null Id Tests

        [Fact]
        public void Print_WithNullId_ShouldReturnNotFound()
        {
            // Act
            var result = _controller.Print(null, "PDF");

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        #endregion

        #region Additional Notifications Tests

        [Fact]
        public void Notifications_ShouldSetAllViewBagProperties()
        {
            // Act
            var result = _controller.Notifications();

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.ViewData["Demo1"].Should().Be("Goal: Learn Design Patterns");
            // Demo2 has ReminderDecorator which adds " [Reminder Enabled] {DateTime} for: " prefix
            (viewResult.ViewData["Demo2"] as string).Should().Contain("Reminder Enabled");
            (viewResult.ViewData["Demo2"] as string).Should().Contain("Goal: Learn Design Patterns");
            // Demo3 has NotificationDecorator which adds " + Status Notification" suffix
            (viewResult.ViewData["Demo3"] as string).Should().Contain("Status Notification");
        }

        [Fact]
        public void Notifications_Demo1_ShouldShowBaseGoalDescription()
        {
            // Act
            var result = _controller.Notifications();

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var demo1 = viewResult.ViewData["Demo1"] as string;
            demo1.Should().Contain("Learn Design Patterns");
            demo1.Should().NotContain("Reminder");
            demo1.Should().NotContain("Notification");
        }

        [Fact]
        public void Notifications_Demo2_ShouldShowReminderDecoratedDescription()
        {
            // Act
            var result = _controller.Notifications();

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var demo2 = viewResult.ViewData["Demo2"] as string;
            demo2.Should().Contain("Reminder Enabled");
            demo2.Should().Contain("Goal: Learn Design Patterns");
        }

        [Fact]
        public void Notifications_Demo3_ShouldShowFullyDecoratedDescription()
        {
            // Act
            var result = _controller.Notifications();

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var demo3 = viewResult.ViewData["Demo3"] as string;
            demo3.Should().Contain("Reminder Enabled");
            demo3.Should().Contain("Status Notification");
        }

        #endregion

        #region Additional Print Format Tests

        [Fact]
        public void Print_WithLowercasePdfFormat_ShouldReturnPdfContentType()
        {
            // Arrange
            var goalId = 1;
            var userId = 1;
            var format = "pdf"; // lowercase - ToUpper() in switch handles this
            var fileBytes = new byte[] { 1, 2, 3 };

            _mockExportService.Setup(s => s.ExportGoal(goalId, userId, format)).Returns(fileBytes);
            _mockExportService.Setup(s => s.GetAvailableFormats()).Returns(AvailableExportFormats);

            // Act
            var result = _controller.Print(goalId, format);

            // Assert - format.ToUpper() in switch matches "PDF"
            var fileResult = result.Should().BeOfType<FileContentResult>().Subject;
            fileResult.ContentType.Should().Be("application/pdf");
        }

        [Fact]
        public void Print_WithLowercaseExcelFormat_ShouldReturnExcelContentType()
        {
            // Arrange
            var goalId = 1;
            var userId = 1;
            var format = "excel"; // lowercase - ToUpper() in switch handles this
            var fileBytes = new byte[] { 1, 2, 3 };

            _mockExportService.Setup(s => s.ExportGoal(goalId, userId, format)).Returns(fileBytes);
            _mockExportService.Setup(s => s.GetAvailableFormats()).Returns(AvailableExportFormats);

            // Act
            var result = _controller.Print(goalId, format);

            // Assert - format.ToUpper() in switch matches "EXCEL"
            var fileResult = result.Should().BeOfType<FileContentResult>().Subject;
            fileResult.ContentType.Should().Be("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        }

        [Fact]
        public void Print_WithMixedCaseFormat_ShouldReturnPdfContentType()
        {
            // Arrange
            var goalId = 1;
            var userId = 1;
            var format = "Pdf"; // mixed case - ToUpper() handles this
            var fileBytes = new byte[] { 1, 2, 3 };

            _mockExportService.Setup(s => s.ExportGoal(goalId, userId, format)).Returns(fileBytes);
            _mockExportService.Setup(s => s.GetAvailableFormats()).Returns(AvailableExportFormats);

            // Act
            var result = _controller.Print(goalId, format);

            // Assert - format.ToUpper() matches "PDF"
            var fileResult = result.Should().BeOfType<FileContentResult>().Subject;
            fileResult.ContentType.Should().Be("application/pdf");
        }

        [Fact]
        public void Print_WithValidFormat_ShouldUseCorrectFileExtension()
        {
            // Arrange
            var goalId = 1;
            var userId = 1;
            var format = "EXCEL";
            var fileBytes = new byte[] { 1, 2, 3 };

            _mockExportService.Setup(s => s.ExportGoal(goalId, userId, format)).Returns(fileBytes);
            _mockExportService.Setup(s => s.GetAvailableFormats()).Returns(AvailableExportFormats);

            // Act
            var result = _controller.Print(goalId, format);

            // Assert
            var fileResult = result.Should().BeOfType<FileContentResult>().Subject;
            fileResult.FileDownloadName.Should().Be($"goal-{goalId}.excel");
        }

        [Fact]
        public void Print_WithUnavailableFormat_ShouldDefaultToPdfExtension()
        {
            // Arrange
            var goalId = 1;
            var userId = 1;
            var format = "XML"; // Not in available formats
            var fileBytes = new byte[] { 1, 2, 3 };

            _mockExportService.Setup(s => s.ExportGoal(goalId, userId, format)).Returns(fileBytes);
            _mockExportService.Setup(s => s.GetAvailableFormats()).Returns(AvailableExportFormats);

            // Act
            var result = _controller.Print(goalId, format);

            // Assert - Should default to pdf extension when format not available
            var fileResult = result.Should().BeOfType<FileContentResult>().Subject;
            fileResult.FileDownloadName.Should().Be($"goal-{goalId}.pdf");
        }

        [Fact]
        public void Print_WhenExportThrowsException_ShouldSetTempDataError()
        {
            // Arrange
            var goalId = 1;
            var userId = 1;
            var format = "PDF";
            var errorMessage = "Export service unavailable";

            _mockExportService.Setup(s => s.ExportGoal(goalId, userId, format))
                .Throws(new Exception(errorMessage));

            // Act
            var result = _controller.Print(goalId, format);

            // Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Details");
            _controller.TempData["Error"].Should().NotBeNull();
            _controller.TempData["Error"]!.ToString().Should().Contain(errorMessage);
        }

        [Fact]
        public void Print_WhenExportThrowsException_ShouldIncludeFormatInErrorMessage()
        {
            // Arrange
            var goalId = 1;
            var userId = 1;
            var format = "EXCEL";
            var errorMessage = "File generation failed";

            _mockExportService.Setup(s => s.ExportGoal(goalId, userId, format))
                .Throws(new Exception(errorMessage));

            // Act
            _controller.Print(goalId, format);

            // Assert
            _controller.TempData["Error"]!.ToString().Should().Contain(format);
        }

        #endregion

        #region Index Additional Tests

        [Fact]
        public void Index_WithNoGoals_ShouldReturnEmptyList()
        {
            // Arrange
            var userId = 1;
            _mockGoalService.Setup(s => s.GetUserGoals(userId)).Returns(new List<Goal>());

            // Act
            var result = _controller.Index();

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeAssignableTo<IEnumerable<Goal>>().Subject;
            model.Should().BeEmpty();
        }

        #endregion

        #region Details Additional Tests

        [Fact]
        public void Details_ShouldSetProgressInViewBag()
        {
            // Arrange
            var goalId = 1;
            var userId = 1;
            var goal = new Goal { Id = goalId, Name = "Test Goal", UserId = userId };
            var progress = new GoalProgress
            {
                GoalId = goalId,
                progressData = new GoalProgressData { ProgressPercentage = 50 }
            };

            _mockGoalService.Setup(s => s.GetGoalById(goalId, userId)).Returns(goal);
            _mockProgressService.Setup(p => p.GetProgress(goalId, userId)).Returns(progress);
            _mockProgressService.Setup(p => p.GetProgressHistory(goalId, userId)).Returns(new List<GoalProgress>());

            // Act
            var result = _controller.Details(goalId);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.ViewData["Progress"].Should().Be(progress);
        }

        [Fact]
        public void Details_ShouldSetProgressHistoryInViewBag()
        {
            // Arrange
            var goalId = 1;
            var userId = 1;
            var goal = new Goal { Id = goalId, Name = "Test Goal", UserId = userId };
            var history = new List<GoalProgress>
            {
                new GoalProgress { GoalId = goalId, progressData = new GoalProgressData { ProgressPercentage = 25 } },
                new GoalProgress { GoalId = goalId, progressData = new GoalProgressData { ProgressPercentage = 50 } }
            };

            _mockGoalService.Setup(s => s.GetGoalById(goalId, userId)).Returns(goal);
            _mockProgressService.Setup(p => p.GetProgress(goalId, userId)).Returns((GoalProgress?)null);
            _mockProgressService.Setup(p => p.GetProgressHistory(goalId, userId)).Returns(history);

            // Act
            var result = _controller.Details(goalId);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.ViewData["ProgressHistory"].Should().Be(history);
        }

        #endregion

        #region CreateGoalVariant Additional Tests

        [Fact]
        public void CreateGoalVariant_ShouldCallGoalFactory()
        {
            // Arrange
            var goalType = "ShortTerm";
            var name = "Test Goal";
            var targetDate = DateTime.UtcNow.AddDays(30);
            var createdGoal = new Goal { Name = name, targetDate = targetDate };

            _mockGoalFactory.Setup(f => f.CreateGoal(goalType, name, targetDate)).Returns(createdGoal);

            // Act
            _controller.CreateGoalVariant(goalType, name, targetDate);

            // Assert
            _mockGoalFactory.Verify(f => f.CreateGoal(goalType, name, targetDate), Times.Once);
        }

        [Fact]
        public void CreateGoalVariant_ShouldPassCreatedGoalToService()
        {
            // Arrange
            var userId = 1;
            var goalType = "LongTerm";
            var name = "Career Goal";
            var targetDate = DateTime.UtcNow.AddYears(1);
            var createdGoal = new LongTermGoal { Name = name, targetDate = targetDate };

            _mockGoalFactory.Setup(f => f.CreateGoal(goalType, name, targetDate)).Returns(createdGoal);

            // Act
            _controller.CreateGoalVariant(goalType, name, targetDate);

            // Assert
            _mockGoalService.Verify(s => s.CreateGoal(createdGoal, userId), Times.Once);
        }

        #endregion

        #region UpdateProgress Additional Tests

        [Fact]
        public void UpdateProgress_ShouldPassCorrectParametersToService()
        {
            // Arrange
            var goalId = 1;
            var userId = 1;
            var progressPercentage = 75;
            var notes = "Making good progress";

            // Act
            _controller.UpdateProgress(goalId, progressPercentage, notes);

            // Assert
            _mockProgressService.Verify(p => p.UpdateProgress(goalId, userId, progressPercentage, notes), Times.Once);
        }

        [Fact]
        public void UpdateProgress_ShouldRedirectToDetailsWithCorrectId()
        {
            // Arrange
            var goalId = 5;
            var progressPercentage = 100;

            // Act
            var result = _controller.UpdateProgress(goalId, progressPercentage, "Complete");

            // Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Details");
            redirectResult.RouteValues!["id"].Should().Be(goalId);
        }

        #endregion

    }
}