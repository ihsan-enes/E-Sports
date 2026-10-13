using ESpor.Application.Common.Models;
using ESpor.Application.Teams.DTOs;
using ESpor.Application.Teams.Handlers;
using ESpor.Application.Teams.Queries;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace ESpor.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TeamsController : ControllerBase
{
    private readonly GetTeamsHandler _getTeamsHandler;
    private readonly GetTeamByIdHandler _getTeamByIdHandler;
    private readonly IValidator<GetTeamsQuery> _validator;

    public TeamsController(
        GetTeamsHandler getTeamsHandler,
        GetTeamByIdHandler getTeamByIdHandler,
        IValidator<GetTeamsQuery> validator)
    {
        _getTeamsHandler = getTeamsHandler;
        _getTeamByIdHandler = getTeamByIdHandler;
        _validator = validator;
    }

    [HttpGet]
    [EndpointSummary("Takımları listeler")]
    [EndpointDescription("Oyun ve isim/kısaltma arama filtrelerine göre takımları sayfalı olarak getirir.")]
    [ProducesResponseType(typeof(PagedResult<TeamDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetTeams(
        [FromQuery] string? game,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetTeamsQuery(game, search, page, pageSize);
        var validationResult = await _validator.ValidateAsync(query, cancellationToken);

        if (!validationResult.IsValid)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Geçersiz İstek Parametreleri",
                Detail = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)),
                Instance = HttpContext.Request.Path
            });
        }

        var result = await _getTeamsHandler.HandleAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [EndpointSummary("Belirtilen takımı detaylarıyla getirir")]
    [EndpointDescription("Benzersiz kimliğe (Id) sahip takımın kadrosunu (roster), son 10 bitmiş maçını, kazanma oranını ve son 5 maçlık form durumunu (W/L) döner.")]
    [ProducesResponseType(typeof(TeamDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTeamById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var team = await _getTeamByIdHandler.HandleAsync(id, cancellationToken);

        if (team == null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Takım Bulunamadı",
                Detail = $"'{id}' kimliğine sahip takım bulunamadı.",
                Instance = HttpContext.Request.Path
            });
        }

        return Ok(team);
    }
}
