using MockApi.Runtime.DataModels.Auditing;

namespace MockApi.Dtos.Auth
{
    public class LoginUserInput
    {
        public string Email { get; set; } = string.Empty;
        [DisableLog]
        public string Password { get; set; } = string.Empty;
    }
}
