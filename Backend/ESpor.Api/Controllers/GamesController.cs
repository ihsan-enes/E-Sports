using ESpor.Application.Games.DTOs;
using ESpor.Application.Games.Handlers;
using Microsoft.AspNetCore.Mvc;

namespace ESpor.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GamesController : ControllerBase
{
    private readonly GetGamesHandler _getGamesHandler;

    public GamesController(GetGamesHandler getGamesHandler)
    {
        _getGamesHandler = getGamesHandler;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<GameDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<GameDto>>> GetGames(CancellationToken cancellationToken)
    {
        var games = await _getGamesHandler.HandleAsync(cancellationToken);
        return Ok(games);
    }
}
