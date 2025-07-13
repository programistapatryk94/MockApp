using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MockApi.Data;
using MockApi.Dtos.Mock;
using MockApi.Extensions;
using MockApi.Helpers;
using MockApi.Localization;
using MockApi.Models;
using MockApi.Runtime.Features;
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
        private readonly IFeatureChecker _featureChecker;
        private readonly ITranslationService _translationService;

        public MocksController(AppDbContext context, IMapper mapper, IAppSession appSession, IFeatureChecker featureChecker, ITranslationService translationService)
        {
            _context = context;
            _mapper = mapper;
            _appSession = appSession;
            _featureChecker = featureChecker;
            _translationService = translationService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MockDto>>> GetMocks([FromQuery] GetMocksInput input)
        {
            var ownerId = await _context.Projects.Where(p => p.Id == input.ProjectId).Select(p => p.UserId).FirstOrDefaultAsync();
            var collaborationEnabled = await _featureChecker.IsEnabledAsync(ownerId, AppFeatures.CollaborationEnabled);

            List<Mock> mocks;

            using (_context.MaybeWithFilterOff(nameof(AppDbContext.ApplyCollaborationFilter), !collaborationEnabled))
            {
                mocks = await _context.Mocks.Where(m => m.ProjectId == input.ProjectId)
                .OrderByDescending(m => m.Enabled)
                .ThenBy(m => m.CreatedAt)
                .ToListAsync();
            }

            var mocksDto = _mapper.Map<IEnumerable<MockDto>>(mocks);

            return Ok(mocksDto);
        }

        [HttpPost]
        public async Task<ActionResult<MockDto>> CreateMock([FromBody] CreateOrUpdateMockInput createMockInput)
        {
            var ownerId = await _context.Projects.IgnoreQueryFilters().Where(p => p.Id == createMockInput.ProjectId).Select(p => p.UserId).FirstOrDefaultAsync();

            var maxMocksLimit = (await _featureChecker.GetValueAsync(ownerId, AppFeatures.MaxMockCreationLimit)).To<int>();

            var collaborationEnabled = await _featureChecker.IsEnabledAsync(ownerId, AppFeatures.CollaborationEnabled);

            using (_context.WithFilterOff(nameof(AppDbContext.ApplyCollaborationFilter)))
            {
                var count = await _context.Mocks.CountAsync(m => m.Project.UserId == ownerId);
                if (count > maxMocksLimit)
                {
                    return BadRequest(_translationService.Translate("MaxMockLimitReached", maxMocksLimit));
                }
            }

            using (_context.MaybeWithFilterOff(nameof(AppDbContext.ApplyCollaborationFilter), !collaborationEnabled))
            {
                if (!await _context.Projects.AnyAsync(p => p.Id == createMockInput.ProjectId))
                {
                    return NotFound(_translationService.Translate("ProjectNotFound"));
                }
            }
            var mock = _mapper.Map<Mock>(createMockInput);
            mock.Id = Guid.NewGuid();
            mock.UserId = _appSession.UserId!.Value;
            _context.Mocks.Add(mock);
            await _context.SaveChangesAsync();

            var mockDto = _mapper.Map<MockDto>(mock);

            return Ok(mockDto);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<MockDto>> UpdateMock(Guid id, [FromBody] CreateOrUpdateMockInput updateMockInput)
        {
            var ownerId = await _context.Projects.IgnoreQueryFilters().Where(p => p.Id == updateMockInput.ProjectId).Select(p => p.UserId).FirstOrDefaultAsync();
            var collaborationEnabled = await _featureChecker.IsEnabledAsync(ownerId, AppFeatures.CollaborationEnabled);

            Mock? mock;

            using (_context.MaybeWithFilterOff(nameof(AppDbContext.ApplyCollaborationFilter), !collaborationEnabled))
            {
                mock = await _context.Mocks.FirstOrDefaultAsync(m => m.Id == id && m.ProjectId == updateMockInput.ProjectId);
            }

            if (null == mock)
            {
                return NotFound(_translationService.Translate("MockNotFound"));
            }

            _mapper.Map(updateMockInput, mock);

            await _context.SaveChangesAsync();

            var mockDto = _mapper.Map<MockDto>(mock);
            return Ok(mockDto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMock(Guid id)
        {
            var ownerId = await _context.Mocks.IgnoreQueryFilters().Where(p => p.Id == id).Select(p => p.Project.UserId).FirstOrDefaultAsync();
            var collaborationEnabled = await _featureChecker.IsEnabledAsync(ownerId, AppFeatures.CollaborationEnabled);

            Mock? mock;

            using (_context.MaybeWithFilterOff(nameof(AppDbContext.ApplyCollaborationFilter), !collaborationEnabled))
            {
                mock = await _context.Mocks.FirstOrDefaultAsync(m => m.Id == id);
            }

            if (null == mock) return NotFound(_translationService.Translate("MockNotFound"));

            _context.Mocks.Remove(mock);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
