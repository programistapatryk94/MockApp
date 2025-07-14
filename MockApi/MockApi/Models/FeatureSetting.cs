using MockApi.Runtime.DataModels.Auditing;
using System.ComponentModel.DataAnnotations;

namespace MockApi.Models
{
    public class FeatureSetting : AuditedEntity<long>
    {
        public const int MaxNameLength = 128;
        public const int MaxValueLength = 2000;

        [Required]
        [MaxLength(MaxNameLength)]
        public string Name { get; set; }

        [Required(AllowEmptyStrings = true)]
        [MaxLength(MaxValueLength)]
        public string Value { get; set; }

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
