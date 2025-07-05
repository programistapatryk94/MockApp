namespace MockApi.Dtos.Project
{
    public class ProjectDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Secret { get; set; } = string.Empty;
        public string ApiPrefix { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Guid UserId { get; set; }
    }
}
