using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MockApi.Controllers;
using MockApi.Data;
using MockApi.Localization;
using MockApi.Runtime.Session;
using Moq;

namespace MockApi.Tests.Controllers
{
    public class PublicMockControllerTests
    {
        private readonly Mock<ITranslationService> _mockTranslationService = new();
        private readonly AppDbContext _context;
        private readonly PublicMockController _controller;

        public PublicMockControllerTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options, Mock.Of<IAppSession>());
            _controller = new PublicMockController(_context, _mockTranslationService.Object);
        }

        [Fact]
        public async Task HandleRequest_WhenNoSubdomain_ReturnsNotFound()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Host = new HostString(""); // brak subdomeny
            var controllerContext = new ControllerContext { HttpContext = httpContext };
            _mockTranslationService.Setup(x => x.Translate("SubdomainNotFound")).Returns("Subdomain not found");

            _controller.ControllerContext = controllerContext;

            // Act
            var result = await _controller.HandleRequest();

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Subdomain not found", notFound.Value);
        }

        [Fact]
        public async Task HandleRequest_WhenProjectNotFound_ReturnsNotFound()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Host = new HostString("fake.mockapi.com");
            _mockTranslationService.Setup(x => x.Translate("ProjectNotFound")).Returns("Project not found");

            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            // Act
            var result = await _controller.HandleRequest();

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Project not found", notFound.Value);
        }

        [Fact]
        public async Task HandleRequest_WhenPathPrefixInvalid_ReturnsNotFound()
        {
            // Arrange
            var project = new MockApi.Models.Project
            {
                Id = Guid.NewGuid(),
                Secret = "test",
                ApiPrefix = "/api"
            };
            await _context.Projects.AddAsync(project);
            await _context.SaveChangesAsync();

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Host = new HostString("test.localhost");
            httpContext.Request.Path = "/wrongprefix/data";
            httpContext.Request.Method = "GET";

            _mockTranslationService.Setup(x => x.Translate("InvalidPathPrefix")).Returns("Invalid path");

            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            // Act
            var result = await _controller.HandleRequest();

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Invalid path", notFound.Value);
        }

        [Fact]
        public async Task HandleRequest_WhenMockFound_ReturnsExpectedMock()
        {
            // Arrange
            var project = new MockApi.Models.Project
            {
                Id = Guid.NewGuid(),
                Secret = "mock",
                ApiPrefix = "/api"
            };

            var mock = new MockApi.Models.Mock
            {
                Id = Guid.NewGuid(),
                ProjectId = project.Id,
                Method = "GET",
                UrlPath = "/endpoint",
                Enabled = true,
                StatusCode = 200,
                ResponseBody = "{\"message\":\"success\"}",
                HeadersJson = "{\"X-Test\":\"HeaderValue\"}"
            };

            await _context.Projects.AddAsync(project);
            await _context.Mocks.AddAsync(mock);
            await _context.SaveChangesAsync();

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Host = new HostString("mock.localhost");
            httpContext.Request.Path = "/api/endpoint";
            httpContext.Request.Method = "GET";

            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            // Act
            var result = await _controller.HandleRequest();

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, objectResult.StatusCode);
            Assert.Equal("{\"message\":\"success\"}", objectResult.Value);
            Assert.Equal("HeaderValue", httpContext.Response.Headers["X-Test"]);
        }

    }
}
