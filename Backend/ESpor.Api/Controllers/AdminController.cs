using ESpor.Application.Sync.Handlers;
using Microsoft.AspNetCore.Mvc;

namespace ESpor.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly TriggerSyncHandler _triggerSyncHandler;
    private readonly IConfiguration _configuration;

    public AdminController(TriggerSyncHandler triggerSyncHandler, IConfiguration configuration)
    {
        _triggerSyncHandler = triggerSyncHandler;
        _configuration = configuration;
    }

    [HttpPost("sync/{job}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> TriggerSync(
        [FromRoute] string job,
        [FromHeader(Name = "X-Admin-Key")] string? adminKey,
        CancellationToken cancellationToken)
    {
        var configuredKey = _configuration["Admin:Key"] ?? _configuration["Admin__Key"];

        if (string.IsNullOrWhiteSpace(configuredKey) || adminKey != configuredKey)
        {
            return Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Yetkisiz Erişim",
                Detail = "Geçersiz veya eksik 'X-Admin-Key' başlığı."
            });
        }

        var result = await _triggerSyncHandler.HandleAsync(job, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Senkronizasyon Başarısız",
                Detail = result.Error
            });
        }

        return Ok(new { message = result.Value });
    }
}
