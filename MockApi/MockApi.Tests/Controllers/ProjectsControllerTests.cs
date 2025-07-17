using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MockApi.Controllers;
using MockApi.Data;
using MockApi.Dtos.Project;
using MockApi.Helpers;
using MockApi.Localization;
using MockApi.Models;
using MockApi.Runtime.Features;
using MockApi.Runtime.Session;
using Moq;

namespace MockApi.Tests.Controllers
{
    public class ProjectsControllerTests
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly Mock<IAppSession> _mockSession = new Mock<IAppSession>();
        private readonly Mock<IFeatureChecker> _mockFeatureChecker = new Mock<IFeatureChecker>();
        private readonly Mock<ITranslationService> _mockTranslation = new Mock<ITranslationService>();
        private readonly ProjectsController _controller;


        public ProjectsControllerTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase("ProjectDbTest")
                .Options;

            _context = new AppDbContext(options, _mockSession.Object);

            // Konfiguracja mappera (możesz podstawić swój profil)
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddMaps(typeof(MockApi.Mappings.MappingProfile));
            });
            _mapper = config.CreateMapper();

            _controller = new ProjectsController(
                _context,
                _mapper,
                _mockSession.Object,
                _mockFeatureChecker.Object,
                _mockTranslation.Object
            );
        }

        [Fact]
        public async Task GetProjectById_ReturnsNotFound_WhenProjectDoesNotExist()
        {
            // Arrange
            var id = Guid.NewGuid();
            _mockTranslation.Setup(x => x.Translate("ProjectNotFound")).Returns("Project not found");

            // Act
            var result = await _controller.GetProjectById(id);

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal("Project not found", notFound.Value);
        }

        [Fact]
        public async Task GetProjectById_ReturnsNotFound_WhenUserIsNotOwner_AndCollaborationIsDisabled()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var callerId = Guid.NewGuid();

            _context.Projects.Add(new Project
            {
                Id = projectId,
                Name = "Private Project",
                CreatorUserId = ownerId
            });
            await _context.SaveChangesAsync();

            _mockSession.Setup(x => x.UserId).Returns(callerId); // nie właściciel
            _mockFeatureChecker.Setup(x => x.IsEnabledAsync(ownerId, AppFeatures.CollaborationEnabled))
                .ReturnsAsync(false);
            _mockTranslation.Setup(x => x.Translate("ProjectNotFound"))
                .Returns("Project not found");

            // Act
            var result = await _controller.GetProjectById(projectId);

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal("Project not found", notFound.Value);
        }

        [Fact]
        public async Task GetProjectById_ReturnsProjectDto_WhenProjectExists()
        {
            // Arrange
            var id = Guid.NewGuid();
            var userId = Guid.NewGuid();
            _mockSession.SetupGet(p => p.UserId).Returns(userId);

            var project = new Project
            {
                Id = id,
                Name = "Test Project",
                CreatorUserId = userId
            };

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            _mockFeatureChecker.Setup(x => x.IsEnabledAsync(userId, "CollaborationEnabled"))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.GetProjectById(id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var dto = Assert.IsType<ProjectDto>(okResult.Value);
            Assert.Equal("Test Project", dto.Name);
        }

        [Fact]
        public async Task GetProjectById_ReturnsProject_WhenCollaborationEnabled_AndUserIsProjectMember_AndUserNotOwner()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var callerId = Guid.NewGuid();

            _mockSession.Setup(x => x.UserId).Returns(callerId);
            _mockFeatureChecker.Setup(x => x.IsEnabledAsync(ownerId, AppFeatures.CollaborationEnabled)).ReturnsAsync(true);

            _context.Users.Add(new User
            {
                Id = ownerId,
                Email = "test@test.pl",
                IsCollaborationEnabled = true
            });

            _context.Projects.Add(new Project
            {
                Id = projectId,
                Name = "Współdzielony projekt",
                CreatorUserId = ownerId
            });

            _context.ProjectMembers.Add(new ProjectMember
            {
                ProjectId = projectId,
                UserId = callerId
            });

            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetProjectById(projectId);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var dto = Assert.IsType<ProjectDto>(ok.Value);
            Assert.Equal("Współdzielony projekt", dto.Name);
        }

        [Fact]
        public async Task GetProjects_ReturnsAllProjects_ForCurrentUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _mockSession.Setup(x => x.UserId).Returns(userId);

            _context.Projects.AddRange(
                new Project { Id = Guid.NewGuid(), Name = "Proj1", CreatorUserId = userId },
                new Project { Id = Guid.NewGuid(), Name = "Proj2", CreatorUserId = userId }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetProjects();

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var list = Assert.IsAssignableFrom<IEnumerable<ProjectDto>>(ok.Value);
            Assert.Equal(2, list.Count());
            Assert.Contains(list, p => p.Name == "Proj1");
            Assert.Contains(list, p => p.Name == "Proj2");
        }

        [Fact]
        public async Task GetProjects_ReturnsEmpty_WhenUserIsNotOwnerOrMember_AndCollaborationEnabled()
        {
            // Arrange
            var callerId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            _mockSession.Setup(x => x.UserId).Returns(callerId);

            _context.Projects.Add(new Project
            {
                Id = Guid.NewGuid(),
                Name = "Cudzy projekt",
                CreatorUserId = ownerId // inny właściciel
            });

            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetProjects();

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var list = Assert.IsAssignableFrom<IEnumerable<ProjectDto>>(ok.Value);
            Assert.Empty(list); // brak widocznych projektów
        }

        [Fact]
        public async Task GetProjects_ReturnsProject_WhenUserIsMember_AndCollaborationEnabled()
        {
            // Arrange
            var callerId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            _mockSession.Setup(x => x.UserId).Returns(callerId);

            _context.Users.Add(new User
            {
                Id = ownerId,
                Email = "test@test.pl",
                IsCollaborationEnabled = true
            });

            _context.Projects.Add(new Project
            {
                Id = projectId,
                Name = "Projekt wspólny",
                CreatorUserId = ownerId
            });

            _context.ProjectMembers.Add(new ProjectMember
            {
                ProjectId = projectId,
                UserId = callerId
            });

            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetProjects();

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var list = Assert.IsAssignableFrom<IEnumerable<ProjectDto>>(ok.Value);
            var project = Assert.Single(list); // tylko jeden projekt widoczny
            Assert.Equal("Projekt wspólny", project.Name);
        }

        [Fact]
        public async Task CreateProject_ReturnsBadRequest_WhenLimitExceeded()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _mockSession.Setup(x => x.UserId).Returns(userId);
            _mockFeatureChecker.Setup(x => x.GetValueAsync(userId, AppFeatures.MaxProjectCreationLimit, true))
                .ReturnsAsync("0"); // limit = 0

            _mockTranslation.Setup(x => x.Translate("MaxProjectLimitReached", 0)).Returns("Limit exceeded");

            var input = new CreateOrUpdateProjectInput
            {
                Name = "New Project"
            };

            // Act
            var result = await _controller.CreateProject(input);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Limit exceeded", badRequest.Value);
        }

        [Fact]
        public async Task CreateProject_ReturnsOk_WhenWithinLimit()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _mockSession.Setup(x => x.UserId).Returns(userId);
            _mockFeatureChecker.Setup(x => x.GetValueAsync(userId, AppFeatures.MaxProjectCreationLimit, true))
                .ReturnsAsync("5"); // limit = 5

            var input = new CreateOrUpdateProjectInput
            {
                Name = "My Project"
            };

            // Act
            var result = await _controller.CreateProject(input);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var dto = Assert.IsType<ProjectDto>(ok.Value);
            Assert.Equal("My Project", dto.Name);
            Assert.NotEqual(Guid.Empty, dto.Id);
        }

        [Fact]
        public async Task UpdateProject_ReturnsNotFound_WhenProjectDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _mockSession.Setup(x => x.UserId).Returns(userId);
            _mockFeatureChecker.Setup(x => x.IsEnabledAsync(userId, AppFeatures.CollaborationEnabled)).ReturnsAsync(false);
            _mockTranslation.Setup(x => x.Translate("ProjectNotFound")).Returns("Not found");

            var id = Guid.NewGuid();
            var input = new CreateOrUpdateProjectInput { Name = "Updated" };

            // Act
            var result = await _controller.UpdateProject(id, input);

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal("Not found", notFound.Value);
        }

        [Fact]
        public async Task UpdateProject_ReturnsUpdatedProject_WhenSuccessful()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _mockSession.Setup(x => x.UserId).Returns(userId);
            _mockFeatureChecker.Setup(x => x.IsEnabledAsync(userId, AppFeatures.CollaborationEnabled)).ReturnsAsync(false);

            var project = new Project
            {
                Id = Guid.NewGuid(),
                Name = "Old Name",
                CreatorUserId = userId
            };

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            var input = new CreateOrUpdateProjectInput { Name = "New Name" };

            // Act
            var result = await _controller.UpdateProject(project.Id, input);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var dto = Assert.IsType<ProjectDto>(ok.Value);
            Assert.Equal("New Name", dto.Name);
        }

        [Fact]
        public async Task DeleteProject_ReturnsNotFound_WhenProjectDoesNotExist()
        {
            var id = Guid.NewGuid();
            _mockTranslation.Setup(x => x.Translate("ProjectNotFound")).Returns("Not found");

            var result = await _controller.DeleteProject(id);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Not found", notFound.Value);
        }

        [Fact]
        public async Task DeleteProject_ReturnsNotFound_WhenUserIsNotOwner()
        {
            var id = Guid.NewGuid();
            var userId = Guid.NewGuid();

            _context.Projects.Add(new Project
            {
                Id = id,
                Name = "Test",
                CreatorUserId = userId
            });
            await _context.SaveChangesAsync();

            _mockSession.Setup(x => x.UserId).Returns(Guid.NewGuid());
            _mockFeatureChecker.Setup(x => x.IsEnabledAsync(userId, AppFeatures.CollaborationEnabled)).ReturnsAsync(false);
            _mockTranslation.Setup(x => x.Translate("ProjectAccessDenied")).Returns("Access denied");

            var result = await _controller.DeleteProject(id);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(notFound.StatusCode, 404);
        }

        [Fact]
        public async Task DeleteProject_DeletesProjectAndMocks_WhenUserIsOwner()
        {
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            _context.Projects.Add(new Project
            {
                Id = projectId,
                Name = "My Project",
                CreatorUserId = userId
            });

            _context.Mocks.Add(new MockApi.Models.Mock
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                UrlPath = "Test Mock"
            });

            await _context.SaveChangesAsync();

            _mockSession.Setup(x => x.UserId).Returns(userId);
            _mockFeatureChecker.Setup(x => x.IsEnabledAsync(userId, AppFeatures.CollaborationEnabled)).ReturnsAsync(false);

            var result = await _controller.DeleteProject(projectId);

            Assert.IsType<NoContentResult>(result);

            Assert.False(_context.Projects.Any(p => p.Id == projectId));
            Assert.False(_context.Mocks.Any(m => m.ProjectId == projectId));
        }

    }

}
