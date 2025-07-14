using MockApi.Runtime.DataModels.Auditing;
using System.ComponentModel.DataAnnotations;

namespace MockApi.Models
{
    public class Mock : AuditedEntity<Guid, User>
    {
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
        public bool Enabled { get; set; } = true;

        public Guid ProjectId { get; set; }
        public virtual Project Project { get; set; }
    }
}
