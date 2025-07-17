using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MockApi.Controllers;
using MockApi.Data;
using MockApi.Dtos.Mock;
using MockApi.Dtos.Project;
using MockApi.Helpers;
using MockApi.Localization;
using MockApi.Models;
using MockApi.Runtime.Features;
using MockApi.Runtime.Session;
using Moq;

namespace MockApi.Tests.Controllers
{
    public class MocksControllerTests
    {
        private readonly Mock<IAppSession> _mockSession = new Mock<IAppSession>();
        private readonly IMapper _mapper;
        private readonly Mock<ITranslationService> _mockTranslationService = new Mock<ITranslationService>();
        private readonly Mock<IFeatureChecker> _mockFeatureChecker = new Mock<IFeatureChecker>();
        private readonly AppDbContext _context;
        private readonly MocksController _controller;

        public MocksControllerTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options, _mockSession.Object);

            // Konfiguracja mappera (możesz podstawić swój profil)
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddMaps(typeof(MockApi.Mappings.MappingProfile));
            });
            _mapper = config.CreateMapper();

            _controller = new MocksController(
                _context,
                _mapper,
                _mockFeatureChecker.Object,
                _mockTranslationService.Object
            );
        }

        [Fact]
        public async Task GetMocks_ReturnsNotFound_WhenProjectDoesNotExist()
        {
            // Arrange
            var input = new GetMocksInput { ProjectId = Guid.NewGuid() };

            _mockTranslationService.Setup(t => t.Translate("ProjectNotFound"))
                .Returns("Projekt nie istnieje");

            // Act
            var result = await _controller.GetMocks(input);

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal("Projekt nie istnieje", notFound.Value);
        }

        [Fact]
        public async Task GetMocks_ReturnsNotFound_WhenCollabDisabled_AndNotOwner()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            _mockTranslationService.Setup(t => t.Translate("ProjectNotFound"))
                .Returns("Projekt nie istnieje");

            _context.Users.Add(new User
            {
                Id = ownerId,
                Email = "test@test.pl",
                IsCollaborationEnabled = true
            });

            _context.Projects.Add(new Project { Id = projectId, CreatorUserId = ownerId });

            _context.Mocks.AddRange(
                new MockApi.Models.Mock { Id = Guid.NewGuid(), ProjectId = projectId, CreatorUserId = userId, UrlPath = "Widoczny" },
                new MockApi.Models.Mock { Id = Guid.NewGuid(), ProjectId = projectId, CreatorUserId = ownerId, UrlPath = "Ukryty" }
            );
            await _context.SaveChangesAsync();

            _mockSession.Setup(s => s.UserId).Returns(userId);
            _mockFeatureChecker.Setup(f => f.IsEnabledAsync(ownerId, AppFeatures.CollaborationEnabled)).ReturnsAsync(false);

            var input = new GetMocksInput { ProjectId = projectId };

            // Act
            var result = await _controller.GetMocks(input);

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal("Projekt nie istnieje", notFound.Value);
        }

        [Fact]
        public async Task GetMocks_ReturnsNotFound_WhenCollabEnabled_AndNotOwner_AndNotProjectMember()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            _mockTranslationService.Setup(t => t.Translate("ProjectNotFound"))
                .Returns("Projekt nie istnieje");

            _context.Users.Add(new User
            {
                Id = ownerId,
                Email = "test@test.pl",
                IsCollaborationEnabled = true
            });

            _context.Projects.Add(new Project { Id = projectId, CreatorUserId = ownerId });

            _context.Mocks.AddRange(
                new MockApi.Models.Mock { Id = Guid.NewGuid(), ProjectId = projectId, CreatorUserId = userId, UrlPath = "Widoczny" },
                new MockApi.Models.Mock { Id = Guid.NewGuid(), ProjectId = projectId, CreatorUserId = ownerId, UrlPath = "Ukryty" }
            );
            await _context.SaveChangesAsync();

            _mockSession.Setup(s => s.UserId).Returns(userId);
            _mockFeatureChecker.Setup(f => f.IsEnabledAsync(ownerId, AppFeatures.CollaborationEnabled)).ReturnsAsync(true);

            var input = new GetMocksInput { ProjectId = projectId };

            // Act
            var result = await _controller.GetMocks(input);

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal("Projekt nie istnieje", notFound.Value);
        }

        [Fact]
        public async Task GetMocks_ReturnsMocks_WhenCollabEnabled_AndNotOwner_AndProjectMember()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            _context.Users.Add(new User
            {
                Id = ownerId,
                Email = "test@test.pl",
                IsCollaborationEnabled = true
            });

            _context.Projects.Add(new Project { Id = projectId, CreatorUserId = ownerId });

            _context.ProjectMembers.Add(new ProjectMember
            {
                ProjectId = projectId,
                UserId = userId
            });

            _context.Mocks.AddRange(
                new MockApi.Models.Mock { Id = Guid.NewGuid(), ProjectId = projectId, CreatorUserId = userId, UrlPath = "Widoczny" },
                new MockApi.Models.Mock { Id = Guid.NewGuid(), ProjectId = projectId, CreatorUserId = ownerId, UrlPath = "Ukryty" }
            );
            await _context.SaveChangesAsync();

            _mockSession.Setup(s => s.UserId).Returns(userId);
            _mockFeatureChecker.Setup(f => f.IsEnabledAsync(ownerId, AppFeatures.CollaborationEnabled)).ReturnsAsync(true);

            var input = new GetMocksInput { ProjectId = projectId };

            // Act
            var result = await _controller.GetMocks(input);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var list = Assert.IsAssignableFrom<IEnumerable<MockDto>>(ok.Value);
            Assert.Equal(2, list.Count());
            Assert.Contains(list, p => p.UrlPath == "Widoczny");
            Assert.Contains(list, p => p.UrlPath == "Ukryty");
        }

        [Fact]
        public async Task CreateMock_ReturnsNotFound_WhenProjectDoesNotExist()
        {
            // Arrange
            var input = new CreateOrUpdateMockInput
            {
                ProjectId = Guid.NewGuid(),
                UrlPath = "Mock"
            };

            _mockTranslationService.Setup(t => t.Translate("ProjectNotFound"))
                .Returns("Projekt nie istnieje");

            // Act
            var result = await _controller.CreateMock(input);

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal("Projekt nie istnieje", notFound.Value);
        }

        [Fact]
        public async Task CreateMock_ReturnsBadRequest_WhenMockLimitReached()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            _context.Projects.Add(new Project { Id = projectId, CreatorUserId = userId });

            // dodajemy 3 mocki – limit będzie 3
            for (int i = 0; i < 3; i++)
            {
                _context.Mocks.Add(new MockApi.Models.Mock
                {
                    Id = Guid.NewGuid(),
                    ProjectId = projectId,
                    CreatorUserId = userId,
                    UrlPath = $"Mock {i}"
                });
            }
            await _context.SaveChangesAsync();

            _mockSession.Setup(s => s.UserId).Returns(userId);
            _mockFeatureChecker.Setup(f => f.GetValueAsync(userId, AppFeatures.MaxMockCreationLimit, true)).ReturnsAsync("3");
            _mockTranslationService.Setup(t => t.Translate("MaxMockLimitReached", 3)).Returns("Limit przekroczony");

            var input = new CreateOrUpdateMockInput { ProjectId = projectId, UrlPath = "NowyMock" };

            // Act
            var result = await _controller.CreateMock(input);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Limit przekroczony", badRequest.Value);
        }

        [Fact]
        public async Task CreateMock_ReturnsNotFound_WhenUserIsMember_AndCollabDisabled()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var memberId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            _context.Projects.Add(new Project { Id = projectId, CreatorUserId = ownerId });
            _context.ProjectMembers.Add(new ProjectMember { ProjectId = projectId, UserId = memberId });
            await _context.SaveChangesAsync();

            _mockSession.Setup(s => s.UserId).Returns(memberId);
            _mockFeatureChecker.Setup(f => f.GetValueAsync(ownerId, AppFeatures.MaxMockCreationLimit, true)).ReturnsAsync("5");
            _mockFeatureChecker.Setup(f => f.IsEnabledAsync(ownerId, AppFeatures.CollaborationEnabled)).ReturnsAsync(false);
            _mockTranslationService.Setup(t => t.Translate("ProjectNotFound")).Returns("Projekt nie istnieje");

            var input = new CreateOrUpdateMockInput { ProjectId = projectId, UrlPath = "Mock" };

            // Act
            var result = await _controller.CreateMock(input);

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal("Projekt nie istnieje", notFound.Value);
        }

        [Fact]
        public async Task CreateMock_ReturnsNotFound_WhenUserNotMember_AndCollabEnabled()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var strangerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            _context.Users.Add(new User
            {
                Id = ownerId,
                Email = "test@test.pl",
                IsCollaborationEnabled = true
            });

            _context.Projects.Add(new Project { Id = projectId, CreatorUserId = ownerId });
            await _context.SaveChangesAsync();

            _mockSession.Setup(s => s.UserId).Returns(strangerId);
            _mockFeatureChecker.Setup(f => f.GetValueAsync(ownerId, AppFeatures.MaxMockCreationLimit, true)).ReturnsAsync("5");
            _mockFeatureChecker.Setup(f => f.IsEnabledAsync(ownerId, AppFeatures.CollaborationEnabled)).ReturnsAsync(true);
            _mockTranslationService.Setup(t => t.Translate("ProjectNotFound")).Returns("Projekt nie istnieje");

            var input = new CreateOrUpdateMockInput { ProjectId = projectId, UrlPath = "Mock" };

            // Act
            var result = await _controller.CreateMock(input);

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal("Projekt nie istnieje", notFound.Value);
        }

        [Fact]
        public async Task CreateMock_ReturnsCreated_WhenUserIsMember_AndCollabEnabled()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var memberId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            _context.Users.Add(new User
            {
                Id = ownerId,
                Email = "test@test.pl",
                IsCollaborationEnabled = true
            });

            _context.Projects.Add(new Project { Id = projectId, CreatorUserId = ownerId });
            _context.ProjectMembers.Add(new ProjectMember { ProjectId = projectId, UserId = memberId });
            await _context.SaveChangesAsync();

            _mockSession.Setup(s => s.UserId).Returns(memberId);
            _mockFeatureChecker.Setup(f => f.IsEnabledAsync(ownerId, AppFeatures.CollaborationEnabled)).ReturnsAsync(true);
            _mockFeatureChecker.Setup(f => f.GetValueAsync(ownerId, AppFeatures.MaxMockCreationLimit, true)).ReturnsAsync("5");

            var input = new CreateOrUpdateMockInput { ProjectId = projectId, UrlPath = "CollaborativeMock" };

            // Act
            var result = await _controller.CreateMock(input);

            // Assert
            var created = Assert.IsType<OkObjectResult>(result.Result);
            var dto = Assert.IsType<MockDto>(created.Value);
            Assert.Equal("CollaborativeMock", dto.UrlPath);
        }

        [Fact]
        public async Task CreateMock_ReturnsCreated_WhenMockCreatedSuccessfully()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            _context.Projects.Add(new Project { Id = projectId, CreatorUserId = userId });
            await _context.SaveChangesAsync();

            _mockSession.Setup(s => s.UserId).Returns(userId);
            _mockFeatureChecker.Setup(f => f.GetValueAsync(userId, AppFeatures.MaxMockCreationLimit, true)).ReturnsAsync("5");

            var input = new CreateOrUpdateMockInput
            {
                ProjectId = projectId,
                UrlPath = "NowyMock"
            };

            // Act
            var result = await _controller.CreateMock(input);

            // Assert
            var created = Assert.IsType<OkObjectResult>(result.Result);
            var dto = Assert.IsType<MockDto>(created.Value);
            Assert.Equal("NowyMock", dto.UrlPath);
        }

        [Fact]
        public async Task UpdateMock_ReturnsNotFound_WhenMockDoesNotExist()
        {
            // Arrange
            var input = new CreateOrUpdateMockInput
            {
                ProjectId = Guid.NewGuid(),
                UrlPath = "Nieistniejący"
            };

            _mockTranslationService.Setup(t => t.Translate("MockNotFound")).Returns("Mock nie istnieje");

            // Act
            var result = await _controller.UpdateMock(Guid.NewGuid(), input);

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal("Mock nie istnieje", notFound.Value);
        }

        [Fact]
        public async Task UpdateMock_ReturnsNotFound_WhenUserNotOwnerOrMember_AndCollabDisabled()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var strangerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var mockId = Guid.NewGuid();

            _context.Projects.Add(new Project { Id = projectId, CreatorUserId = ownerId });
            _context.Mocks.Add(new MockApi.Models.Mock
            {
                Id = mockId,
                ProjectId = projectId,
                CreatorUserId = ownerId,
                UrlPath = "Oryginalny"
            });
            await _context.SaveChangesAsync();

            _mockSession.Setup(s => s.UserId).Returns(strangerId);
            _mockFeatureChecker.Setup(f => f.IsEnabledAsync(ownerId, AppFeatures.CollaborationEnabled)).ReturnsAsync(false);
            _mockTranslationService.Setup(t => t.Translate("MockNotFound")).Returns("Mock nie istnieje");

            var input = new CreateOrUpdateMockInput
            {
                ProjectId = projectId,
                UrlPath = "Zmieniony"
            };

            // Act
            var result = await _controller.UpdateMock(mockId, input);

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal("Mock nie istnieje", notFound.Value);
        }

        [Fact]
        public async Task UpdateMock_ReturnsOk_WhenOwnerUpdatesMock()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var mockId = Guid.NewGuid();

            _context.Projects.Add(new Project { Id = projectId, CreatorUserId = ownerId });
            _context.Mocks.Add(new MockApi.Models.Mock
            {
                Id = mockId,
                ProjectId = projectId,
                CreatorUserId = ownerId,
                UrlPath = "Stary"
            });
            await _context.SaveChangesAsync();

            _mockSession.Setup(s => s.UserId).Returns(ownerId);
            _mockFeatureChecker.Setup(f => f.IsEnabledAsync(ownerId, AppFeatures.CollaborationEnabled)).ReturnsAsync(false);

            var input = new CreateOrUpdateMockInput
            {
                ProjectId = projectId,
                UrlPath = "Nowy"
            };

            // Act
            var result = await _controller.UpdateMock(mockId, input);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var dto = Assert.IsType<MockDto>(ok.Value);
            Assert.Equal("Nowy", dto.UrlPath);
        }

        [Fact]
        public async Task UpdateMock_ReturnsOk_WhenMemberUpdatesMock_AndCollabEnabled()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var memberId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var mockId = Guid.NewGuid();

            _context.Users.Add(new User
            {
                Id = ownerId,
                Email = "test@test.pl",
                IsCollaborationEnabled = true
            });

            _context.Projects.Add(new Project { Id = projectId, CreatorUserId = ownerId });
            _context.ProjectMembers.Add(new ProjectMember { ProjectId = projectId, UserId = memberId });
            _context.Mocks.Add(new MockApi.Models.Mock
            {
                Id = mockId,
                ProjectId = projectId,
                CreatorUserId = ownerId,
                UrlPath = "Stary"
            });
            await _context.SaveChangesAsync();

            _mockSession.Setup(s => s.UserId).Returns(memberId);
            _mockFeatureChecker.Setup(f => f.IsEnabledAsync(ownerId, AppFeatures.CollaborationEnabled)).ReturnsAsync(true);

            var input = new CreateOrUpdateMockInput
            {
                ProjectId = projectId,
                UrlPath = "Zaktualizowany przez członka"
            };

            // Act
            var result = await _controller.UpdateMock(mockId, input);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var dto = Assert.IsType<MockDto>(ok.Value);
            Assert.Equal("Zaktualizowany przez członka", dto.UrlPath);
        }

        [Fact]
        public async Task DeleteMock_ReturnsNotFound_WhenMockDoesNotExist()
        {
            // Arrange
            var nonExistingId = Guid.NewGuid();
            _mockTranslationService.Setup(t => t.Translate("MockNotFound")).Returns("Mock nie istnieje");

            // Act
            var result = await _controller.DeleteMock(nonExistingId);

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Mock nie istnieje", notFound.Value);
        }

        [Fact]
        public async Task DeleteMock_ReturnsNotFound_WhenUserNotOwnerOrMember()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var strangerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var mockId = Guid.NewGuid();

            _context.Projects.Add(new Project { Id = projectId, CreatorUserId = ownerId });
            _context.Mocks.Add(new MockApi.Models.Mock
            {
                Id = mockId,
                ProjectId = projectId,
                CreatorUserId = ownerId,
                UrlPath = "Mock"
            });
            await _context.SaveChangesAsync();

            _mockSession.Setup(s => s.UserId).Returns(strangerId);
            _mockFeatureChecker.Setup(f => f.IsEnabledAsync(ownerId, AppFeatures.CollaborationEnabled)).ReturnsAsync(false);
            _mockTranslationService.Setup(t => t.Translate("MockNotFound")).Returns("Mock nie istnieje");

            // Act
            var result = await _controller.DeleteMock(mockId);

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Mock nie istnieje", notFound.Value);
        }

        [Fact]
        public async Task DeleteMock_ReturnsNoContent_WhenOwnerDeletes()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var mockId = Guid.NewGuid();

            _context.Projects.Add(new Project { Id = projectId, CreatorUserId = ownerId });
            _context.Mocks.Add(new MockApi.Models.Mock
            {
                Id = mockId,
                ProjectId = projectId,
                CreatorUserId = ownerId,
                UrlPath = "Mock"
            });
            await _context.SaveChangesAsync();

            _mockSession.Setup(s => s.UserId).Returns(ownerId);
            _mockFeatureChecker.Setup(f => f.IsEnabledAsync(ownerId, AppFeatures.CollaborationEnabled)).ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteMock(mockId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteMock_ReturnsNoContent_WhenMemberDeletes_AndCollabEnabled()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var memberId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var mockId = Guid.NewGuid();

            _context.Users.Add(new User
            {
                Id = ownerId,
                Email = "test@test.pl",
                IsCollaborationEnabled = true
            });

            _context.Projects.Add(new Project { Id = projectId, CreatorUserId = ownerId });
            _context.ProjectMembers.Add(new ProjectMember { ProjectId = projectId, UserId = memberId });
            _context.Mocks.Add(new MockApi.Models.Mock
            {
                Id = mockId,
                ProjectId = projectId,
                CreatorUserId = ownerId,
                UrlPath = "Mock"
            });
            await _context.SaveChangesAsync();

            _mockSession.Setup(s => s.UserId).Returns(memberId);
            _mockFeatureChecker.Setup(f => f.IsEnabledAsync(ownerId, AppFeatures.CollaborationEnabled)).ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteMock(mockId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

    }
}
