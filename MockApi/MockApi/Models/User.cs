namespace MockApi.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = "";
        public string PasswordHash { get; set; } = "";

        public virtual ICollection<Mock> Mocks { get; set; } = new List<Mock>();
        public virtual ICollection<ProjectMember> ProjectMembers { get; set; } = new List<ProjectMember>();
        public virtual Subscription? Subscription { get; set; }
    }
}
