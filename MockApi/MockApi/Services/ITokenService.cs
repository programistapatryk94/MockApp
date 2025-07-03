using MockApi.Models;

namespace MockApi.Services
{
    public interface ITokenService
    {
        string GenerateToken(User user);
    }
}
