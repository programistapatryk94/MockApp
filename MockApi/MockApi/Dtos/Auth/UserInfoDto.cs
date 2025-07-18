namespace MockApi.Dtos.Auth
{
    public class UserInfoDto
    {
        public LocalizationConfigurationDto Localization { get; set; }
        public Dictionary<string, object> Features { get; set; }
    }
}
