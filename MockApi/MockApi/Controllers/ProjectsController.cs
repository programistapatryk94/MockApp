using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MockApi.Data;
using MockApi.Dtos.Project;
using MockApi.Helpers;
using MockApi.Models;

namespace MockApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProjectsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public ProjectsController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProjectDto>> GetProjectById(Guid id)
        {
            var userId = GetUserId();

            var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            if (null == project) return NotFound();

            var projectDto = _mapper.Map<ProjectDto>(project);

            return Ok(projectDto);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProjectDto>>> GetProjects()
        {
            var userId = GetUserId();

            var projects = await _context.Projects.Where(p => p.UserId == userId).ToListAsync();
            var mappedProjects = _mapper.Map<IEnumerable<ProjectDto>>(projects);

            return Ok(mappedProjects);
        }

        [HttpPost]
        public async Task<ActionResult<ProjectDto>> CreateProject([FromBody] CreateProjectInput createProjectInput)
        {
            var userId = GetUserId();

            var project = _mapper.Map<Project>(createProjectInput);
            project.Id = Guid.NewGuid();
            project.UserId = userId;
            project.Secret = SecretGenerator.Generate();

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            var projectDto = _mapper.Map<ProjectDto>(project);

            return Ok(projectDto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(Guid id)
        {
            var userId = GetUserId();
            var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);
            if (null == project) return NotFound();

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private Guid GetUserId()
        {
            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            return userId;
        }
    }
}
