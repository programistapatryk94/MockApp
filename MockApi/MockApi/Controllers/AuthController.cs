using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MockApi.Data;
using MockApi.Dtos.Auth;
using MockApi.Helpers;
using MockApi.Localization;
using MockApi.Migrations;
using MockApi.Models;
using MockApi.Runtime.Features;
using MockApi.Runtime.Session;
using MockApi.Services;
using System.Globalization;

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
        private readonly IFeatureManager _featureManager;

        public AuthController(AppDbContext context, ITokenService tokenService, ITranslationService translationService, ILanguageManager languageManager, IAppSession appSession, IFeatureManager featureManager)
        {
            _context = context;
            _tokenService = tokenService;
            _translationService = translationService;
            _languageManager = languageManager;
            _appSession = appSession;
            _featureManager = featureManager;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] CreateUserInput user)
        {
            if (await _context.Users.AnyAsync(u => u.Email == user.Email))
            {
                return BadRequest(_translationService.Translate("email_exists"));
            }

            var newUser = new User
            {
                Email = user.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.Password)
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return Ok(new { Token = _tokenService.GenerateToken(newUser) });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserInput loginData)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginData.Email);
            if (null == user || !BCrypt.Net.BCrypt.Verify(loginData.Password, user.PasswordHash))
            {
                return Unauthorized(_translationService.Translate("invalid_credentials"));
            }

            return Ok(new { token = _tokenService.GenerateToken(user) });
        }

        [HttpPost("changeLanguage")]
        [Authorize]
        public async Task<IActionResult> ChangeLanguage([FromBody] ChangeLanguageInput input)
        {
            var userId = _appSession.UserId;
            if (!userId.HasValue)
                return Unauthorized();

            var feature = _featureManager.Get(AppFeatures.DefaultLanguage);
            var defaultLanguage = feature.DefaultValue;
            var userFeature = await _context.FeatureSettings
                .FirstOrDefaultAsync(f => f.UserId == userId.Value && f.Name == AppFeatures.DefaultLanguage);

            // Jeśli użytkownik wybrał język domyślny – usuń nadpisane ustawienie
            if (input.LanguageName.Equals(defaultLanguage, StringComparison.OrdinalIgnoreCase))
            {
                if (userFeature != null)
                {
                    _context.FeatureSettings.Remove(userFeature);
                }
            }
            else
            {
                if (userFeature != null)
                {
                    userFeature.Value = input.LanguageName;
                }
                else
                {
                    _context.FeatureSettings.Add(new FeatureSetting
                    {
                        UserId = userId.Value,
                        CreatorUserId = userId.Value,
                        Name = AppFeatures.DefaultLanguage,
                        Value = input.LanguageName
                    });
                }
            }

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("me")]
        public async Task<ActionResult<UserInfoDto>> Me()
        {
            var currentCulture = CultureInfo.CurrentUICulture;

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
                }
            };

            return Ok(dto);
        }
    }
}
