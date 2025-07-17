using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MockApi.Models;
using MockApi.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MockApi.Tests.Services
{
    public class TokenServiceTests
    {
        private readonly TokenService _service;

        public TokenServiceTests()
        {
            var inMemorySettings = new Dictionary<string, string> {
            {"Jwt:Key", "super-secret-test-key-1234567890"},
            {"Jwt:Issuer", "TestIssuer"},
            {"Jwt:Audience", "TestAudience"}
        };

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings!)
                .Build();

            _service = new TokenService(config);
        }

        [Fact]
        public void GenerateToken_ReturnsValidJwt()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com"
            };

            // Act
            var token = _service.GenerateToken(user);

            // Assert
            Assert.False(string.IsNullOrEmpty(token));

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes("super-secret-test-key-1234567890");

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = "TestIssuer",
                ValidAudience = "TestAudience",
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            var nameId = jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
            var email = jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;

            Assert.Equal(user.Id.ToString(), nameId);
            Assert.Equal(user.Email, email);
        }

        [Fact]
        public void GenerateToken_WithoutKey_ThrowsException()
        {
            // Arrange
            var config = new ConfigurationBuilder().Build(); // brak klucza
            var service = new TokenService(config);
            var user = new User { Id = Guid.NewGuid(), Email = "x@example.com" };

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => service.GenerateToken(user));
        }

    }

}
