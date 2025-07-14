using MockApi.Runtime.DataModels.Auditing;
using System.ComponentModel.DataAnnotations;

namespace MockApi.Models
{
    public class Project : AuditedEntity<Guid, User>
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        [Required]
        [MaxLength(64)]
        public string Secret { get; set; } = string.Empty;
        [MaxLength(50)]
        public string ApiPrefix { get; set; } = string.Empty;
        //public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        //public Guid UserId { get; set; }
        //public virtual User User { get; set; }
        public virtual ICollection<Mock> Mocks { get; set; } = new List<Mock>();
        public virtual ICollection<ProjectMember> ProjectMembers { get; set; } = new List<ProjectMember>();
    }
}
