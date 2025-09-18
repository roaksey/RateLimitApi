using Microsoft.AspNetCore.Mvc;
using RateLimitApi.Data;

namespace RateLimitApi.Controllers;

[ApiController]
[Route("admin/[controller]")]
public class AdminController : ControllerBase
{
    private readonly SqlRepository _repo;

    public AdminController(SqlRepository repo) => _repo = repo;

    [HttpGet("blocks")]
    public async Task<IActionResult> GetBlocks()
        => Ok(await _repo.GetActiveBlocksAsync());

    [HttpPost("unblock")]
    public async Task<IActionResult> Unblock([FromBody] UnblockRequest req)
    {
        var cleared = await _repo.UnblockAsync(req);
        return Ok(new { ok = true, cleared });
    }
}
