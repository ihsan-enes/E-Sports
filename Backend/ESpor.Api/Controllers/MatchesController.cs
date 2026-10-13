using ESpor.Application.Common.Models;
using ESpor.Application.Matches.DTOs;
using ESpor.Application.Matches.Handlers;
using ESpor.Application.Matches.Queries;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace ESpor.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MatchesController : ControllerBase
{
    private readonly GetLiveMatchesHandler _getLiveMatchesHandler;
    private readonly GetUpcomingMatchesHandler _getUpcomingMatchesHandler;
    private readonly GetMatchByIdHandler _getMatchByIdHandler;
    private readonly IValidator<GetLiveMatchesQuery> _liveValidator;
    private readonly IValidator<GetUpcomingMatchesQuery> _upcomingValidator;

    public MatchesController(
        GetLiveMatchesHandler getLiveMatchesHandler,
        GetUpcomingMatchesHandler getUpcomingMatchesHandler,
        GetMatchByIdHandler getMatchByIdHandler,
        IValidator<GetLiveMatchesQuery> liveValidator,
        IValidator<GetUpcomingMatchesQuery> upcomingValidator)
    {
        _getLiveMatchesHandler = getLiveMatchesHandler;
        _getUpcomingMatchesHandler = getUpcomingMatchesHandler;
        _getMatchByIdHandler = getMatchByIdHandler;
        _liveValidator = liveValidator;
        _upcomingValidator = upcomingValidator;
    }

    [HttpGet("live")]
    [EndpointSummary("Canlı maçları listeler")]
    [EndpointDescription("Şu anda canlı olarak devam eden maçları getirir. İsteğe bağlı oyun filtresi alır (lol, cs2, valorant).")]
    [ProducesResponseType(typeof(IReadOnlyList<MatchDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetLiveMatches(
        [FromQuery] string? game,
        CancellationToken cancellationToken)
    {
        var query = new GetLiveMatchesQuery(game);
        var validationResult = await _liveValidator.ValidateAsync(query, cancellationToken);

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

        var matches = await _getLiveMatchesHandler.HandleAsync(query, cancellationToken);
        return Ok(matches);
    }

    [HttpGet("upcoming")]
    [EndpointSummary("Gelecek maçları listeler")]
    [EndpointDescription("Gelecek maçları sayfalama ve tarih filtreleriyle birlikte getirir.")]
    [ProducesResponseType(typeof(PagedResult<MatchDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetUpcomingMatches(
        [FromQuery] string? game,
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetUpcomingMatchesQuery(game, from, to, page, pageSize);
        var validationResult = await _upcomingValidator.ValidateAsync(query, cancellationToken);

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

        var result = await _getUpcomingMatchesHandler.HandleAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [EndpointSummary("Belirtilen maçı getirir")]
    [EndpointDescription("Benzersiz kimliğe (Id) sahip maçın detay bilgilerini döner.")]
    [ProducesResponseType(typeof(MatchDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMatchById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var match = await _getMatchByIdHandler.HandleAsync(id, cancellationToken);

        if (match == null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Maç Bulunamadı",
                Detail = $"'{id}' kimliğine sahip maç bulunamadı.",
                Instance = HttpContext.Request.Path
            });
        }

        return Ok(match);
    }
}
