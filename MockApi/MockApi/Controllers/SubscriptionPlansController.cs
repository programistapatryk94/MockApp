using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MockApi.Data;
using MockApi.Dtos.SubscriptionPlan;
using MockApi.Localization;
using MockApi.Runtime.Exceptions;
using MockApi.Runtime.Session;
using MockApi.Services;
using MockApi.Services.Models;
using System.Globalization;

namespace MockApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SubscriptionPlansController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IAppSession _appSession;
        private readonly ITranslationService _translationService;
        private readonly IUserManager _userManager;

        public SubscriptionPlansController(AppDbContext context, IMapper mapper, IAppSession appSession, IUserManager userManager, ITranslationService translationService)
        {
            _context = context;
            _mapper = mapper;
            _appSession = appSession;
            _userManager = userManager;
            _translationService = translationService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SubscriptionPlanDto>>> GetAll()
        {
            var allowedCurrency = GetAllowedCurrency();

            var items = await _context.SubscriptionPlans
                .Include(sp => sp.Prices.Where(p => p.Currency == allowedCurrency))
                .ToListAsync();

            var dto = _mapper.Map<IEnumerable<SubscriptionPlanDto>>(items);

            return Ok(dto);
        }

        [HttpGet("currentSubscription")]
        public async Task<CurrentSubscriptionInfo?> GetCurrentSubscription()
        {
            var subscriptionPlanPrice = await _userManager.GetCurrentSubscriptionAsync(_appSession.UserId!.Value);

            return subscriptionPlanPrice;
        }

        private string GetAllowedCurrency()
        {
            var lang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            return lang == "pl" ? "PLN" : "USD";
        }
    }
}
