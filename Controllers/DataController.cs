using Microsoft.AspNetCore.Mvc;
using RateLimitApi.Services;

namespace RateLimitApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DataController : ControllerBase
{
    private readonly RateLimitService _rateLimit;

    public DataController(RateLimitService rateLimit) => _rateLimit = rateLimit;

    [HttpGet]
    public async Task<IActionResult> GetData()
    {
        var apiKey = Request.Headers["X-Api-Key"].ToString();
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        var result = await _rateLimit.CheckAndProcessRequest(apiKey, ip, HttpContext.Request.Path);

        if (!result.Allowed)
            return StatusCode(429, new { result.Message });

        return Ok(new { message = "Hello, world!", apiKey, ip });
    }
}
