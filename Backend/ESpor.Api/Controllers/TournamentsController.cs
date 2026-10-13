using ESpor.Application.Common.Models;
using ESpor.Application.Tournaments.DTOs;
using ESpor.Application.Tournaments.Handlers;
using ESpor.Application.Tournaments.Queries;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace ESpor.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TournamentsController : ControllerBase
{
    private readonly GetTournamentsHandler _getTournamentsHandler;
    private readonly GetTournamentByIdHandler _getTournamentByIdHandler;
    private readonly IValidator<GetTournamentsQuery> _validator;

    public TournamentsController(
        GetTournamentsHandler getTournamentsHandler,
        GetTournamentByIdHandler getTournamentByIdHandler,
        IValidator<GetTournamentsQuery> validator)
    {
        _getTournamentsHandler = getTournamentsHandler;
        _getTournamentByIdHandler = getTournamentByIdHandler;
        _validator = validator;
    }

    [HttpGet]
    [EndpointSummary("Turnuvaları listeler")]
    [EndpointDescription("Oyun ve durum (upcoming, running, finished) filtrelerine göre turnuvaları sayfalı olarak listeler.")]
    [ProducesResponseType(typeof(PagedResult<TournamentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetTournaments(
        [FromQuery] string? game,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetTournamentsQuery(game, status, page, pageSize);
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

        var result = await _getTournamentsHandler.HandleAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [EndpointSummary("Belirtilen turnuvayı getirir")]
    [EndpointDescription("Benzersiz kimliğe (Id) sahip turnuvanın detay bilgilerini döner.")]
    [ProducesResponseType(typeof(TournamentDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTournamentById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var tournament = await _getTournamentByIdHandler.HandleAsync(id, cancellationToken);

        if (tournament == null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Turnuva Bulunamadı",
                Detail = $"'{id}' kimliğine sahip turnuva bulunamadı.",
                Instance = HttpContext.Request.Path
            });
        }

        return Ok(tournament);
    }
}
