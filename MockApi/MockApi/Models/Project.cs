using System.ComponentModel.DataAnnotations;

namespace MockApi.Models
{
    public class Project
    {
        public Guid Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public string Secret { get; set; } = string.Empty;
        public string ApiPrefix { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Guid UserId { get; set; }
        public virtual User User { get; set; }
        public virtual ICollection<Mock> Mocks { get; set; } = new List<Mock>();
    }
}
