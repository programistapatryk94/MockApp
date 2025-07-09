using System.ComponentModel.DataAnnotations;

namespace MockApi.Dtos.Project
{
    public class CreateOrUpdateProjectInput
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        [MaxLength(50)]
        public string ApiPrefix { get; set; } = string.Empty;
    }
}
