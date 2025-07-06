namespace MockApi.Dtos.Mock
{
    public class MockDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid ProjectId { get; set; }
        public string UrlPath { get; set; } = "";
        public string Method { get; set; } = "GET";
        public int StatusCode { get; set; } = 200;
        public string ResponseBody { get; set; } = "{}";
        public string? HeadersJson { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool Enabled { get; set; }
    }
}
