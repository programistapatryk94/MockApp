using System.ComponentModel.DataAnnotations;

namespace MockApi.Dtos.Auth
{
    public class ChangeLanguageInput
    {
        [Required]
        public string LanguageName { get; set; }
    }
}
