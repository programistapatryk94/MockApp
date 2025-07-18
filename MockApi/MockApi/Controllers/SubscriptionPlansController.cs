using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MockApi.Data;
using MockApi.Dtos.SubscriptionPlan;

namespace MockApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SubscriptionPlansController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public SubscriptionPlansController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SubscriptionPlanDto>>> GetAll()
        {
            var items = await _context.SubscriptionPlans.Include(p => p.Prices).ToListAsync();

            var dto = _mapper.Map<IEnumerable<SubscriptionPlanDto>>(items);

            return Ok(dto);
        }
    }
}
