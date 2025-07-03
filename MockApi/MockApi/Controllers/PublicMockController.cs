using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MockApi.Data;

namespace MockApi.Controllers
{
    [ApiController]
    [Route("mock/{*path}")]
    public class PublicMockController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PublicMockController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [HttpPost]
        [HttpPut]
        [HttpDelete]
        public async Task<IActionResult> Handle(string path)
        {
            var method = HttpContext.Request.Method;

            var mock = await _context.Mocks.OrderByDescending(m => m.CreatedAt)
                .FirstOrDefaultAsync(_ => _.UrlPath == "/" + path && _.Method == method);

            if (null == mock) return NotFound();

            if(!string.IsNullOrWhiteSpace(mock.HeadersJson))
            {
                var headers = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(mock.HeadersJson);

                foreach(var h in headers!)
                {
                    Response.Headers[h.Key] = h.Value;
                }
            }

            return StatusCode(mock.StatusCode, mock.ResponseBody);

        }
    }
}
