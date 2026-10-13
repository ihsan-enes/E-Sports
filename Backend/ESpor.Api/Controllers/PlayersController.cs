using ESpor.Application.Players.DTOs;
using ESpor.Application.Players.Handlers;
using Microsoft.AspNetCore.Mvc;

namespace ESpor.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlayersController : ControllerBase
{
    private readonly GetPlayerByIdHandler _getPlayerByIdHandler;

    public PlayersController(GetPlayerByIdHandler getPlayerByIdHandler)
    {
        _getPlayerByIdHandler = getPlayerByIdHandler;
    }

    [HttpGet("{id:guid}")]
    [EndpointSummary("Belirtilen oyuncuyu getirir")]
    [EndpointDescription("Benzersiz kimliğe (Id) sahip oyuncunun takma adını (nickname), rolünü, takımını ve temel bilgilerini döner.")]
    [ProducesResponseType(typeof(PlayerDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPlayerById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var player = await _getPlayerByIdHandler.HandleAsync(id, cancellationToken);

        if (player == null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Oyuncu Bulunamadı",
                Detail = $"'{id}' kimliğine sahip oyuncu bulunamadı.",
                Instance = HttpContext.Request.Path
            });
        }

        return Ok(player);
    }
}
