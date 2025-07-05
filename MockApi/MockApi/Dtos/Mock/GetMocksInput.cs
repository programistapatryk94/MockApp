using System.ComponentModel.DataAnnotations;

namespace MockApi.Dtos.Mock
{
    public class GetMocksInput
    {
        [Required]
        public Guid ProjectId { get; set; } = Guid.NewGuid();
    }
}
