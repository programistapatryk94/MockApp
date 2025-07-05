using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MockApi.Data;
using System.Text.Json;

namespace MockApi.Controllers
{
    [ApiController]
    [EnableCors("AllowAll")]
    public class PublicMockController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PublicMockController(AppDbContext context)
        {
            _context = context;
        }

        [Route("{*path}")]
        [HttpGet]
        [HttpPost]
        [HttpPut]
        [HttpDelete]
        [HttpPatch]
        public async Task<IActionResult> HandleRequest()
        {
            var host = HttpContext.Request.Host.Host;
            var subdomain = host.Split('.').FirstOrDefault();

            if (string.IsNullOrWhiteSpace(subdomain))
            {
                return NotFound(new { error = "Subdomena nieznaleziona" });
            }

            var project = await _context.Projects.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Secret == subdomain);

            if (null == project)
            {
                return NotFound("Projekt nie znaleziony");
            }

            var method = HttpContext.Request.Method.ToUpper();
            var fullPath = HttpContext.Request.Path.Value ?? "";

            var prefix = string.IsNullOrWhiteSpace(project.ApiPrefix) ? "/" : project.ApiPrefix;

            if (!fullPath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return NotFound("Zły prefix ścieżki");
            }

            var pathWithoutPrefix = fullPath[prefix.Length..];

            var urlPath = string.IsNullOrWhiteSpace(pathWithoutPrefix)
    ? "/"
    : "/" + pathWithoutPrefix.TrimStart('/');

            var mock = await _context.Mocks.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.ProjectId == project.Id && p.Method == method && p.UrlPath == urlPath);

            if (null == mock)
            {
                return NotFound("Mock nie znaleziony");
            }

            if (!string.IsNullOrWhiteSpace(mock.HeadersJson))
            {
                try
                {
                    var headers = JsonSerializer.Deserialize<Dictionary<string, string>>(mock.HeadersJson);
                    if (headers != null)
                    {
                        foreach (var kv in headers)
                            Response.Headers[kv.Key] = kv.Value;
                    }
                }
                catch { }
            }

            return StatusCode(mock.StatusCode, mock.ResponseBody);
        }
    }
}
