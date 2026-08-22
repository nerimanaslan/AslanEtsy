using AslanEtsy.Application.DTOs.Sync;
using AslanEtsy.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AslanEtsy.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SyncController : ControllerBase
{
    private readonly IEtsySyncService _syncService;

    public SyncController(IEtsySyncService syncService)
    {
        _syncService = syncService;
    }

    [HttpPost("account/{id:int}")]
    public async Task<ActionResult<SyncResultDto>> SyncAccount(int id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _syncService.SyncAccountOrdersAsync(id, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("all")]
    public async Task<ActionResult<List<SyncResultDto>>> SyncAll(CancellationToken cancellationToken)
    {
        var results = await _syncService.SyncAllActiveAccountsAsync(cancellationToken);
        return Ok(results);
    }

    [HttpGet("logs")]
    public async Task<ActionResult<IReadOnlyList<SyncLogDto>>> GetLogs([FromQuery] int? accountId, [FromQuery] int limit = 50, CancellationToken cancellationToken = default)
    {
        var logs = await _syncService.GetSyncLogsAsync(accountId, limit, cancellationToken);
        return Ok(logs);
    }
}
