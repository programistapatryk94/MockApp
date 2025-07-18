using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MockApi.Data;
using MockApi.Dtos.Auth;
using MockApi.Localization;
using MockApi.Models;
using MockApi.Runtime.Features;
using MockApi.Runtime.Session;
using MockApi.Services;

namespace MockApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ITokenService _tokenService;
        private readonly ITranslationService _translationService;
        private readonly ILanguageManager _languageManager;
        private readonly IAppSession _appSession;
        private readonly ILanguageService _languageService;
        private readonly IUserManager _userManager;
        private readonly IFeatureManager _featureManager;

        public AuthController(AppDbContext context, ITokenService tokenService, ITranslationService translationService, ILanguageManager languageManager, IAppSession appSession, ILanguageService languageService, IUserManager userManager, IFeatureManager featureManager)
        {
            _context = context;
            _tokenService = tokenService;
            _translationService = translationService;
            _languageManager = languageManager;
            _appSession = appSession;
            _languageService = languageService;
            _userManager = userManager;
            _featureManager = featureManager;
        }

        [HttpPost("register")]
        public async Task<ActionResult<RegisterResultDto>> Register([FromBody] CreateUserInput user)
        {
            if (await _context.Users.AnyAsync(u => u.NormalizedEmailAddress == user.Email.ToUpperInvariant()))
            {
                return BadRequest(_translationService.Translate("email_exists"));
            }

            var newUser = new User
            {
                Email = user.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.Password)
            };
            newUser.SetNormalizedNames();

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return Ok(new RegisterResultDto { Token = _tokenService.GenerateToken(newUser) });
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResultDto>> Login([FromBody] LoginUserInput loginData)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.NormalizedEmailAddress == loginData.Email.ToUpperInvariant());
            if (null == user || !BCrypt.Net.BCrypt.Verify(loginData.Password, user.PasswordHash))
            {
                return Unauthorized(_translationService.Translate("invalid_credentials"));
            }

            return Ok(new LoginResultDto { Token = _tokenService.GenerateToken(user) });
        }

        [HttpPost("changeLanguage")]
        [Authorize]
        public async Task<IActionResult> ChangeLanguage([FromBody] ChangeLanguageInput input)
        {
            await _languageService.ChangeLanguageAsync(input.LanguageName, _appSession.UserId!.Value);

            return Ok();
        }

        [HttpGet("me")]
        public async Task<ActionResult<UserInfoDto>> Me()
        {
            var currentCulture = _languageManager.CurrentLanguage;

            var features = _featureManager.GetAll();
            Dictionary<string, string> userFeatures = new Dictionary<string, string>();
            if(_appSession.UserId.HasValue)
            {
                userFeatures = await _userManager.GetAllFeaturesAsync(_appSession.UserId.Value);
            }

            Dictionary<string, object> allFeatures = new Dictionary<string, object>();

            foreach (Feature feature in features)
            {
                string camelCaseKey = char.ToLowerInvariant(feature.Name[0]) + feature.Name.Substring(1);

                // Jeśli użytkownik ma nadpisaną wartość, weź ją, jeśli nie — użyj domyślnej
                if (userFeatures.TryGetValue(feature.Name, out var userValue))
                {
                    allFeatures[camelCaseKey] = feature.GetTypedValue(userValue);
                }
                else
                {
                    allFeatures[camelCaseKey] = feature.GetTypedValue();
                }
            }

            var dto = new UserInfoDto
            {
                Localization = new LocalizationConfigurationDto
                {
                    CurrentCulture = new CurrentCultureConfigDto
                    {
                        Name = currentCulture.Name,
                        DisplayName = currentCulture.DisplayName,
                    },
                    Languages = _languageManager.GetLanguages().ToList()
                },
                Features = allFeatures
            };

            return Ok(dto);
        }
    }
}
