using CareerTrack.Controllers;
using CareerTrack.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CareerTrack.Tests.UnitTests.Controllers
{
    public class HomeControllerTests
    {
        [Fact]
        public void Index_ShouldReturnView()
        {
            //Arrange
            var loggerMock = new Mock<ILogger<HomeController>>();
            var controller = new HomeController(loggerMock.Object);

            //Act
            var result = controller.Index();

            //Assert
            result.Should().BeOfType<ViewResult>();
        }

        [Fact]
        public void Privacy_ShouldReturnView()
        {
            var loggerMock = new Mock<ILogger<HomeController>>();
            var controller = new HomeController(loggerMock.Object);

            var result = controller.Privacy();

            result.Should().BeOfType<ViewResult>();
        }

        [Fact]
        public void Error_ShouldReturnView_WithErrorViewModel_AndRequestId()
        {
            //Arrange
            var loggerMock = new Mock<ILogger<HomeController>>();
            var controller = new HomeController(loggerMock.Object);

            var httpContext = new DefaultHttpContext();
            httpContext.TraceIdentifier = "trace-123";

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            //Act
            var result = controller.Error();

            //Assert
            result.Should().BeOfType<ViewResult>();

            var viewResult = (ViewResult)result;
            viewResult.Model.Should().BeOfType<ErrorViewModel>();

            var model = (ErrorViewModel)viewResult.Model!;
            model.RequestId.Should().Be("trace-123");
            model.ShowRequestId.Should().BeTrue();
        }
    }
}