using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MockApi.Data;
using MockApi.Dtos;
using MockApi.Models;
using MockApi.Services;

namespace MockApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ITokenService _tokenService;

        public AuthController(AppDbContext context, ITokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] CreateUserInput user)
        {
            if (await _context.Users.AnyAsync(u => u.Email == user.Email))
            {
                return BadRequest("Email already registered");
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
                return Unauthorized("Invalid credentials");
            }

            return Ok(new { token = _tokenService.GenerateToken(user) });
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var user = await _context.Users.FindAsync(Guid.Parse(userId));
            return Ok(new { user!.Email });
        }
    }
}
