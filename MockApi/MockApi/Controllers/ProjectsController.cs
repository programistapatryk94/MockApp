using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MockApi.Data;
using MockApi.Dtos.Project;
using MockApi.Helpers;
using MockApi.Models;
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

        public ProjectsController(AppDbContext context, IMapper mapper, IAppSession appSession)
        {
            _context = context;
            _mapper = mapper;
            _appSession = appSession;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProjectDto>> GetProjectById(Guid id)
        {
            var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id);

            if (null == project) return NotFound();

            var projectDto = _mapper.Map<ProjectDto>(project);

            return Ok(projectDto);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProjectDto>>> GetProjects()
        {
            var projects = await _context.Projects.ToListAsync();
            var mappedProjects = _mapper.Map<IEnumerable<ProjectDto>>(projects);

            return Ok(mappedProjects);
        }

        [HttpPost]
        public async Task<ActionResult<ProjectDto>> CreateProject([FromBody] CreateOrUpdateProjectInput createProjectInput)
        {
            var userId = _appSession.UserId!.Value;

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
            var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id && p.UserId == _appSession.UserId);

            if (null == project)
            {
                return NotFound();
            }

            _mapper.Map(updateProjectInput, project);

            await _context.SaveChangesAsync();

            var projectDto = _mapper.Map<ProjectDto>(project);
            return Ok(projectDto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(Guid id)
        {
            var isOwner = await _context.Projects.AnyAsync(p => p.Id == id && p.UserId == _appSession.UserId);

            if (!isOwner)
            {
                return NotFound();
            }

            var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id);
            if (null == project) return NotFound();

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
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
