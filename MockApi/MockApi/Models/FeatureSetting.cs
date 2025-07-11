using System.ComponentModel.DataAnnotations;

namespace MockApi.Models
{
    public class FeatureSetting
    {
        public const int MaxNameLength = 128;
        public const int MaxValueLength = 2000;

        [Key]
        public long Id { get; set; }

        [Required]
        [MaxLength(MaxNameLength)]
        public string Name { get; set; }

        [Required(AllowEmptyStrings = true)]
        [MaxLength(MaxValueLength)]
        public string Value { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Guid? CreatorUserId { get; set; }
        public Guid UserId { get; set; }

        public FeatureSetting()
        {

        }

        public FeatureSetting(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }
}
