using CareerTrack.Controllers;
using CareerTrack.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Diagnostics;

namespace CareerTrack.Tests.UnitTests.Controllers
{
    public class HomeControllerTests
    {
        private readonly Mock<ILogger<HomeController>> _mockLogger;
        private readonly HomeController _controller;

        public HomeControllerTests()
        {
            _mockLogger = new Mock<ILogger<HomeController>>();
            _controller = new HomeController(_mockLogger.Object);
        }

        [Fact]
        public void Index_ReturnsViewResult()
        {
            // Act
            var result = _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewName); // Uses default view name
        }

        [Fact]
        public void Privacy_ReturnsViewResult()
        {
            // Act
            var result = _controller.Privacy();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewName); // Uses default view name
        }

        [Fact]
        public void Error_ReturnsViewResultWithErrorViewModel()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.TraceIdentifier = "test-trace-id";
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Act
            var result = _controller.Error();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ErrorViewModel>(viewResult.Model);
            Assert.NotNull(model.RequestId);
        }

        [Fact]
        public void Error_UsesTraceIdentifierWhenActivityCurrentIsNull()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.TraceIdentifier = "test-trace-123";
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Ensure Activity.Current is null
            Activity.Current = null;

            // Act
            var result = _controller.Error();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ErrorViewModel>(viewResult.Model);
            Assert.Equal("test-trace-123", model.RequestId);
        }

        [Fact]
        public void Error_HasCorrectResponseCacheAttribute()
        {
            // Arrange
            var methodInfo = typeof(HomeController).GetMethod(nameof(HomeController.Error));

            // Act
            var attributes = methodInfo?.GetCustomAttributes(typeof(ResponseCacheAttribute), false);

            // Assert
            Assert.NotNull(attributes);
            Assert.Single(attributes);
            var cacheAttribute = attributes[0] as ResponseCacheAttribute;
            Assert.NotNull(cacheAttribute);
            Assert.Equal(0, cacheAttribute.Duration);
            Assert.Equal(ResponseCacheLocation.None, cacheAttribute.Location);
            Assert.True(cacheAttribute.NoStore);
        }

        //[Fact]
        //public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
        //{
        //    // Act & Assert
        //    Assert.Throws<ArgumentNullException>(() => new HomeController(null!));
        //}

        [Fact]
        public void Index_DoesNotRequireAuthentication()
        {
            // Arrange
            var methodInfo = typeof(HomeController).GetMethod(nameof(HomeController.Index));

            // Act
            var authorizeAttributes = methodInfo?.GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), false);

            // Assert
            Assert.NotNull(authorizeAttributes);
            Assert.Empty(authorizeAttributes); // No [Authorize] attribute
        }

        [Fact]
        public void Privacy_DoesNotRequireAuthentication()
        {
            // Arrange
            var methodInfo = typeof(HomeController).GetMethod(nameof(HomeController.Privacy));

            // Act
            var authorizeAttributes = methodInfo?.GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), false);

            // Assert
            Assert.NotNull(authorizeAttributes);
            Assert.Empty(authorizeAttributes); // No [Authorize] attribute
        }
    }
}