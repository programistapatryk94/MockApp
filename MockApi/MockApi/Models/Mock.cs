using System.ComponentModel.DataAnnotations;

namespace MockApi.Models
{
    public class Mock
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        [Required]
        [MaxLength(200)]
        public string UrlPath { get; set; } = "";
        [Required]
        [MaxLength(10)]
        public string Method { get; set; } = "GET";
        [Required]
        [Range(100, 599)]
        public int StatusCode { get; set; } = 200;
        [Required]
        public string ResponseBody { get; set; } = "{}";
        [MaxLength(1000)]
        public string? HeadersJson { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool Enabled { get; set; } = true;

        public virtual User? User { get; set; }
        public Guid ProjectId { get; set; }
        public virtual Project Project { get; set; }
    }
}
