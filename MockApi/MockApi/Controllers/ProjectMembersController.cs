using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MockApi.Data;
using MockApi.Dtos.ProjectMember;
using MockApi.Models;
using MockApi.Runtime.Session;

namespace MockApi.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/projects/{projectId}/members")]
    public class ProjectMembersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IAppSession _appSession;
        private readonly IMapper _mapper;

        public ProjectMembersController(AppDbContext context, IAppSession appSession, IMapper mapper)
        {
            _context = context;
            _appSession = appSession;
            _mapper = mapper;
        }

        /// <summary>
        /// GET: api/projects/{projectId}/collaborators
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProjectMemberDto>>> GetMembers(Guid projectId)
        {
            var isOwner = await _context.Projects.AnyAsync(p => p.Id == projectId && p.UserId == _appSession.UserId);

            if (!isOwner)
            {
                return NotFound();
            }

            var users = await _context.ProjectMembers
                .Where(pc => pc.ProjectId == projectId)
                .Select(pc => pc.User)
                .ToListAsync();

            var outputDto = _mapper.Map<IEnumerable<ProjectMemberDto>>(users);

            return Ok(outputDto);
        }

        /// <summary>
        /// POST: api/projects/{projectId}/collaborators
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<ProjectMemberDto>> AddMember(Guid projectId, [FromBody] AddProjectMemberInput input)
        {
            var isOwner = await _context.Projects.AnyAsync(p => p.Id == projectId && p.UserId == _appSession.UserId);

            if (!isOwner)
            {
                return NotFound();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == input.Email);

            if(null == user)
            {
                return NotFound("Błędny adres e-mail");
            }

            if (user.Id == _appSession.UserId)
            {
                return BadRequest("Nie można dodać siebie jako współpracownika");
            }

            var exists = await _context.ProjectMembers.AnyAsync(pc => pc.ProjectId == projectId && pc.UserId == user.Id);

            if(exists)
            {
                return BadRequest("Użytkownik jest już współpracownikiem");
            }

            _context.ProjectMembers.Add(new ProjectMember
            {
                ProjectId = projectId,
                UserId = user.Id
            });

            await _context.SaveChangesAsync();

            var projectMemberDto = _mapper.Map<ProjectMemberDto>(user);

            return Ok(projectMemberDto);
        }

        /// <summary>
        /// DELETE: api/projects/{projectId}/collaborators/{userId}
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>
        [HttpDelete("{userId}")]
        public async Task<IActionResult> RemoveMember(Guid projectId, Guid userId)
        {
            var isOwner = await _context.Projects.AnyAsync(p => p.Id == projectId && p.UserId == _appSession.UserId);

            if (!isOwner)
            {
                return NotFound();
            }

            var entity = await _context.ProjectMembers
                .FirstOrDefaultAsync(pc => pc.ProjectId == projectId && pc.UserId == userId);

            if(null == entity)
            {
                return NotFound();
            }

            _context.ProjectMembers.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
