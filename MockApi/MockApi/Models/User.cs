using MockApi.Runtime.DataModels;
using System.ComponentModel.DataAnnotations;

namespace MockApi.Models
{
    public class User : Entity
    {
        public const int MaxEmailAddressLength = 256;
        public const int MaxPasswordLength = 128;
        public const int MaxPlainPasswordLength = 32;

        [Required]
        [StringLength(MaxEmailAddressLength)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(MaxEmailAddressLength)]
        public string NormalizedEmailAddress { get; set; } = string.Empty;

        [Required]
        [StringLength(MaxPasswordLength)]
        public string PasswordHash { get; set; } = string.Empty;

        //public virtual ICollection<Mock> Mocks { get; set; } = new List<Mock>();
        public virtual ICollection<ProjectMember> ProjectMembers { get; set; } = new List<ProjectMember>();
        public virtual Subscription? Subscription { get; set; }
        /// <summary>
        /// Flaga ustawiana, gdy użytkownik ma aktywną subskrypcję
        /// </summary>
        public bool IsCollaborationEnabled { get; set; }

        public void SetNormalizedNames()
        {
            NormalizedEmailAddress = Email.ToUpperInvariant();
        }
    }
}
