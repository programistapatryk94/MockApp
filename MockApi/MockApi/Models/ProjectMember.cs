namespace MockApi.Models
{
    public class ProjectMember
    {
        public Guid ProjectId { get; set; }
        public Guid UserId { get; set; }

        public virtual Project Project { get; set; }
        public virtual User User { get; set; }
    }
}
