using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MockApi.Controllers;
using MockApi.Data;
using MockApi.Dtos.Auth;
using MockApi.Dtos.ProjectMember;
using MockApi.Localization;
using MockApi.Models;
using MockApi.Runtime.Session;
using Moq;

namespace MockApi.Tests.Controllers
{
    public class ProjectMembersControllerTests
    {
        private readonly Mock<IAppSession> _mockSession = new();
        private readonly Mock<ITranslationService> _mockTranslationService = new();
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly ProjectMembersController _controller;

        public ProjectMembersControllerTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options, _mockSession.Object);

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddMaps(typeof(MockApi.Mappings.MappingProfile)); // <- Twój profil AutoMappera
            });
            _mapper = config.CreateMapper();

            _controller = new ProjectMembersController(
                _context,
                _mockSession.Object,
                _mapper,
                _mockTranslationService.Object
            );
        }

        [Fact]
        public async Task GetMembers_WhenUserIsOwner_ReturnsProjectMembers()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var owner = new User { Id = ownerId, Email = "owner@example.com", NormalizedEmailAddress = "OWNER@EXAMPLE.COM" };
            var member = new User { Id = Guid.NewGuid(), Email = "member@example.com", NormalizedEmailAddress = "MEMBER@EXAMPLE.COM" };

            await _context.Users.AddRangeAsync(owner, member);
            await _context.Projects.AddAsync(new Project { Id = projectId, CreatorUserId = ownerId });
            await _context.ProjectMembers.AddAsync(new ProjectMember { ProjectId = projectId, UserId = member.Id });

            await _context.SaveChangesAsync();

            _mockSession.Setup(s => s.UserId).Returns(ownerId);

            // Act
            var result = await _controller.GetMembers(projectId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var members = Assert.IsAssignableFrom<IEnumerable<ProjectMemberDto>>(okResult.Value);
            var el = Assert.Single(members);
            Assert.Equal("member@example.com", el.Email);
        }

        [Fact]
        public async Task GetMembers_WhenUserIsNotOwner_ReturnsNotFound()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            await _context.Users.AddRangeAsync(
                new User { Id = ownerId, Email = "owner@example.com" },
                new User { Id = otherUserId, Email = "other@example.com" }
            );

            await _context.Projects.AddAsync(new Project { Id = projectId, CreatorUserId = ownerId });

            await _context.SaveChangesAsync();

            _mockSession.Setup(s => s.UserId).Returns(otherUserId);
            _mockTranslationService.Setup(t => t.Translate("ProjectAccessDenied")).Returns("Access denied");

            // Act
            var result = await _controller.GetMembers(projectId);

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal("Access denied", notFound.Value);
        }

        [Fact]
        public async Task AddMember_WhenUserIsOwnerAndValid_AddsMember()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var newMemberId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var owner = new User { Id = ownerId, Email = "owner@example.com", NormalizedEmailAddress = "OWNER@EXAMPLE.COM" };
            var newMember = new User { Id = newMemberId, Email = "member@example.com", NormalizedEmailAddress = "MEMBER@EXAMPLE.COM" };

            await _context.Users.AddRangeAsync(owner, newMember);
            await _context.Projects.AddAsync(new Project { Id = projectId, CreatorUserId = ownerId });

            await _context.SaveChangesAsync();

            _mockSession.Setup(s => s.UserId).Returns(ownerId);

            var input = new AddProjectMemberInput
            {
                Email = "member@example.com"
            };

            // Act
            var result = await _controller.AddMember(projectId, input);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var dto = Assert.IsType<ProjectMemberDto>(ok.Value);
            Assert.Equal(newMemberId, dto.UserId);
            Assert.Equal("member@example.com", dto.Email);
        }

        [Fact]
        public async Task AddMember_WhenUserIsNotOwner_ReturnsNotFound()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            await _context.Users.AddRangeAsync(
                new User { Id = ownerId, Email = "owner@example.com" },
                new User { Id = otherUserId, Email = "other@example.com" }
            );

            await _context.Projects.AddAsync(new Project { Id = projectId, CreatorUserId = ownerId });

            await _context.SaveChangesAsync();

            _mockSession.Setup(s => s.UserId).Returns(otherUserId);
            _mockTranslationService.Setup(t => t.Translate("ProjectAccessDenied")).Returns("Access denied");

            var input = new AddProjectMemberInput
            {
                Email = "member@example.com"
            };

            // Act
            var result = await _controller.AddMember(projectId, input);

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal("Access denied", notFound.Value);
        }

        [Fact]
        public async Task AddMember_WhenUserTriesToAddSelf_ReturnsBadRequest()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var user = new User { Id = userId, Email = "user@example.com", NormalizedEmailAddress = "USER@EXAMPLE.COM" };

            await _context.Users.AddAsync(user);
            await _context.Projects.AddAsync(new Project { Id = projectId, CreatorUserId = userId });

            await _context.SaveChangesAsync();

            _mockSession.Setup(s => s.UserId).Returns(userId);
            _mockTranslationService.Setup(t => t.Translate("CannotAddSelfAsCollaborator")).Returns("Cannot add yourself");

            var input = new AddProjectMemberInput
            {
                Email = "user@example.com"
            };

            // Act
            var result = await _controller.AddMember(projectId, input);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Cannot add yourself", badRequest.Value);
        }

        [Fact]
        public async Task AddMember_WhenUserWithEmailDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var owner = new User { Id = ownerId, Email = "owner@example.com", NormalizedEmailAddress = "OWNER@EXAMPLE.COM" };

            await _context.Users.AddAsync(owner);
            await _context.Projects.AddAsync(new Project { Id = projectId, CreatorUserId = ownerId });

            await _context.SaveChangesAsync();

            _mockSession.Setup(s => s.UserId).Returns(ownerId);
            _mockTranslationService.Setup(t => t.Translate("UserNotFoundByEmail")).Returns("User not found");

            var input = new AddProjectMemberInput
            {
                Email = "missing@example.com"
            };

            // Act
            var result = await _controller.AddMember(projectId, input);

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal("User not found", notFound.Value);
        }

        [Fact]
        public async Task AddMember_WhenUserAlreadyInProject_ReturnsBadRequest()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var memberId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var owner = new User { Id = ownerId, Email = "owner@example.com", NormalizedEmailAddress = "OWNER@EXAMPLE.COM" };
            var member = new User { Id = memberId, Email = "member@example.com", NormalizedEmailAddress = "MEMBER@EXAMPLE.COM" };

            await _context.Users.AddRangeAsync(owner, member);
            await _context.Projects.AddAsync(new Project { Id = projectId, CreatorUserId = ownerId });
            await _context.ProjectMembers.AddAsync(new ProjectMember { ProjectId = projectId, UserId = memberId });

            await _context.SaveChangesAsync();

            _mockSession.Setup(s => s.UserId).Returns(ownerId);
            _mockTranslationService.Setup(t => t.Translate("UserAlreadyCollaborator")).Returns("Already member");

            var input = new AddProjectMemberInput
            {
                Email = "member@example.com"
            };

            // Act
            var result = await _controller.AddMember(projectId, input);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Already member", badRequest.Value);
        }

        [Fact]
        public async Task RemoveMember_WhenUserIsOwnerAndMemberExists_RemovesMember()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var memberId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            await _context.Users.AddRangeAsync(
                new User { Id = ownerId, Email = "owner@example.com" },
                new User { Id = memberId, Email = "member@example.com" }
            );

            await _context.Projects.AddAsync(new Project { Id = projectId, CreatorUserId = ownerId });

            await _context.ProjectMembers.AddAsync(new ProjectMember
            {
                ProjectId = projectId,
                UserId = memberId
            });

            await _context.SaveChangesAsync();

            _mockSession.Setup(s => s.UserId).Returns(ownerId);

            // Act
            var result = await _controller.RemoveMember(projectId, memberId);

            // Assert
            Assert.IsType<OkResult>(result);

            var memberStillExists = await _context.ProjectMembers
                .AnyAsync(m => m.ProjectId == projectId && m.UserId == memberId);

            Assert.False(memberStillExists);
        }

        [Fact]
        public async Task RemoveMember_WhenUserIsNotOwner_ReturnsNotFound()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var callerId = Guid.NewGuid();
            var memberId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            await _context.Users.AddRangeAsync(
                new User { Id = ownerId, Email = "owner@example.com" },
                new User { Id = callerId, Email = "caller@example.com" },
                new User { Id = memberId, Email = "member@example.com" }
            );

            await _context.Projects.AddAsync(new Project { Id = projectId, CreatorUserId = ownerId });

            await _context.ProjectMembers.AddAsync(new ProjectMember
            {
                ProjectId = projectId,
                UserId = memberId
            });

            await _context.SaveChangesAsync();

            _mockSession.Setup(s => s.UserId).Returns(callerId);
            _mockTranslationService.Setup(t => t.Translate("ProjectAccessDenied")).Returns("Access denied");

            // Act
            var result = await _controller.RemoveMember(projectId, memberId);

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Access denied", notFound.Value);
        }

        [Fact]
        public async Task RemoveMember_WhenMemberNotFound_ReturnsNotFound()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var memberId = Guid.NewGuid(); // nie zostanie dodany
            var projectId = Guid.NewGuid();

            await _context.Users.AddAsync(new User { Id = ownerId, Email = "owner@example.com" });
            await _context.Projects.AddAsync(new Project { Id = projectId, CreatorUserId = ownerId });

            await _context.SaveChangesAsync();

            _mockSession.Setup(s => s.UserId).Returns(ownerId);
            _mockTranslationService.Setup(t => t.Translate("CollaboratorNotFound")).Returns("Member not found");

            // Act
            var result = await _controller.RemoveMember(projectId, memberId);

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Member not found", notFound.Value);
        }
    }
}
