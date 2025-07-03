using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MockApi.Data;
using MockApi.Dtos;
using MockApi.Models;

namespace MockApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MockController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public MockController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetMocks()
        {
            var userId = GetUserId();
            var mocks = await _context.Mocks.Where(m => m.UserId == userId).ToListAsync();
            var mocksDto = _mapper.Map<IEnumerable<MockDto>>(mocks);

            return Ok(mocksDto);
        }

        [HttpPost]
        public async Task<IActionResult> CreateMock([FromBody] CreateMockInput createMockInput)
        {
            var userId = GetUserId();

            var count = await _context.Mocks.CountAsync(m => m.UserId == userId);
            if (count >= 3) return BadRequest("Limit 3 mocków w darmowym planie");

            var mock = _mapper.Map<Mock>(createMockInput);
            mock.Id = Guid.NewGuid();
            mock.UserId = userId;
            _context.Mocks.Add(mock);
            await _context.SaveChangesAsync();

            var mockDto = _mapper.Map<MockDto>(mock);

            return Ok(mockDto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMock(Guid id)
        {
            var userId = GetUserId();
            var mock = await _context.Mocks.FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);
            if (null == mock) return NotFound();

            _context.Mocks.Remove(mock);
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
