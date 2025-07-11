using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MockApi.Data;
using MockApi.Dtos.Project;
using MockApi.Extensions;
using MockApi.Helpers;
using MockApi.Localization;
using MockApi.Models;
using MockApi.Runtime.Features;
using MockApi.Runtime.Session;

namespace MockApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProjectsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IAppSession _appSession;
        private readonly IFeatureChecker _featureChecker;
        private readonly ITranslationService _translationService;

        public ProjectsController(AppDbContext context, IMapper mapper, IAppSession appSession, IFeatureChecker featureChecker, ITranslationService translationService)
        {
            _context = context;
            _mapper = mapper;
            _appSession = appSession;
            _featureChecker = featureChecker;
            _translationService = translationService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProjectDto>> GetProjectById(Guid id)
        {
            var ownerId = await _context.Projects.IgnoreQueryFilters().Where(p => p.Id == id).Select(p => p.UserId).FirstOrDefaultAsync();
            var collaborationEnabled = await _featureChecker.IsEnabledAsync(ownerId, AppFeatures.CollaborationEnabled);

            Project? project;

            using(_context.MaybeWithFilterOff(nameof(AppDbContext.ApplyCollaborationFilter), !collaborationEnabled))
            {
                project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id);
            }

            if (null == project) return NotFound(_translationService.Translate("ProjectNotFound"));

            var projectDto = _mapper.Map<ProjectDto>(project);

            return Ok(projectDto);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProjectDto>>> GetProjects()
        {
            var ownerId = _appSession.UserId!.Value;
            var collaborationEnabled = await _featureChecker.IsEnabledAsync(ownerId, AppFeatures.CollaborationEnabled);

            List<Project> projects;

            using(_context.MaybeWithFilterOff(nameof(AppDbContext.ApplyCollaborationFilter), !collaborationEnabled))
            {
                projects = await _context.Projects.ToListAsync();
            }

            var mappedProjects = _mapper.Map<IEnumerable<ProjectDto>>(projects);

            return Ok(mappedProjects);
        }

        [HttpPost]
        public async Task<ActionResult<ProjectDto>> CreateProject([FromBody] CreateOrUpdateProjectInput createProjectInput)
        {
            var userId = _appSession.UserId!.Value;

            var maxProjectLimit = (await _featureChecker.GetValueAsync(userId, AppFeatures.MaxProjectCreationLimit)).To<int>();
            int projectsCount = 0;

            using (_context.WithFilterOff(nameof(AppDbContext.ApplyCollaborationFilter)))
            {
                projectsCount = await _context.Projects.CountAsync();
            }

            if(projectsCount > maxProjectLimit)
            {
                return BadRequest(_translationService.Translate("MaxProjectLimitReached", maxProjectLimit));
            }

            var project = _mapper.Map<Project>(createProjectInput);
            project.Id = Guid.NewGuid();
            project.UserId = userId;
            project.Secret = SecretGenerator.Generate();

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            var projectDto = _mapper.Map<ProjectDto>(project);

            return Ok(projectDto);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ProjectDto>> UpdateProject(Guid id, [FromBody] CreateOrUpdateProjectInput updateProjectInput)
        {
            var ownerId = _appSession.UserId!.Value;
            var collaborationEnabled = await _featureChecker.IsEnabledAsync(ownerId, AppFeatures.CollaborationEnabled);

            Project? project;

            using(_context.MaybeWithFilterOff(nameof(AppDbContext.ApplyCollaborationFilter), !collaborationEnabled))
            {
                project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id && p.UserId == _appSession.UserId);
            }

            if (null == project)
            {
                return NotFound(_translationService.Translate("ProjectNotFound"));
            }

            _mapper.Map(updateProjectInput, project);

            await _context.SaveChangesAsync();

            var projectDto = _mapper.Map<ProjectDto>(project);
            return Ok(projectDto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(Guid id)
        {
            var ownerId = await _context.Projects.Where(p => p.Id == id).Select(p => p.UserId).FirstOrDefaultAsync();
            var collaborationEnabled = await _featureChecker.IsEnabledAsync(ownerId, AppFeatures.CollaborationEnabled);

            Project? project;

            using(_context.MaybeWithFilterOff(nameof(AppDbContext.ApplyCollaborationFilter), !collaborationEnabled))
            {
                var isOwner = await _context.Projects.AnyAsync(p => p.Id == id && p.UserId == _appSession.UserId);

                if (!isOwner)
                {
                    return NotFound(_translationService.Translate("ProjectAccessDenied"));
                }

                project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id);
            }

            if (null == project) return NotFound(_translationService.Translate("ProjectNotFound"));

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var mocks = _context.Mocks.Where(m => m.ProjectId == id);
                _context.Mocks.RemoveRange(mocks);

                _context.Projects.Remove(project);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return NoContent();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, _translationService.Translate("ProjectDeleteError"));
            }
        }
    }
}
