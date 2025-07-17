using MockApi.Runtime.DataModels.Auditing;

namespace MockApi.Dtos.Auth
{
    public class CreateUserInput
    {
        public string Email { get; set; } = "";
        [DisableLog]
        public string Password { get; set; } = "";
    }
}
