using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MockApi.Data;
using MockApi.Dtos.Mock;
using MockApi.Models;
using MockApi.Runtime.Session;

namespace MockApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MocksController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IAppSession _appSession;

        public MocksController(AppDbContext context, IMapper mapper, IAppSession appSession)
        {
            _context = context;
            _mapper = mapper;
            _appSession = appSession;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MockDto>>> GetMocks([FromQuery] GetMocksInput input)
        {
            var mocks = await _context.Mocks.Where(m => m.ProjectId == input.ProjectId).ToListAsync();
            var mocksDto = _mapper.Map<IEnumerable<MockDto>>(mocks);

            return Ok(mocksDto);
        }

        [HttpPost]
        public async Task<ActionResult<MockDto>> CreateMock([FromBody] CreateOrUpdateMockInput createMockInput)
        {
            var userId = _appSession.UserId!.Value;

            //var count = await _context.Mocks.CountAsync(m => m.UserId == userId);
            //if (count >= 3) return BadRequest("Limit 3 mocków w darmowym planie");
            if (!await _context.Projects.AnyAsync(p => p.Id == createMockInput.ProjectId))
            {
                return NotFound();
            }

            var mock = _mapper.Map<Mock>(createMockInput);
            mock.Id = Guid.NewGuid();
            mock.UserId = userId;
            _context.Mocks.Add(mock);
            await _context.SaveChangesAsync();

            var mockDto = _mapper.Map<MockDto>(mock);

            return Ok(mockDto);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<MockDto>> UpdateMock(Guid id, [FromBody] CreateOrUpdateMockInput updateMockInput)
        {
            var mock = await _context.Mocks.FirstOrDefaultAsync(m => m.Id == id && m.ProjectId == updateMockInput.ProjectId);

            if (null == mock)
            {
                return NotFound();
            }

            _mapper.Map(updateMockInput, mock);

            await _context.SaveChangesAsync();

            var mockDto = _mapper.Map<MockDto>(mock);
            return Ok(mockDto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMock(Guid id)
        {
            var mock = await _context.Mocks.FirstOrDefaultAsync(m => m.Id == id);
            if (null == mock) return NotFound();

            _context.Mocks.Remove(mock);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
