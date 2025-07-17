using Microsoft.Extensions.Configuration;
using MockApi.Models;
using MockApi.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MockApi.Tests.Services
{
    public class TokenServiceTests
    {
        private readonly TokenService _service;

        public TokenServiceTests()
        {
            var inMemorySettings = new Dictionary<string, string> {
            {"Jwt:Key", "super-secret-test-key-1234567890"}
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

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            Assert.Equal(user.Id.ToString(), jwt.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
            Assert.Equal(user.Email, jwt.Claims.First(c => c.Type == ClaimTypes.Email).Value);
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
