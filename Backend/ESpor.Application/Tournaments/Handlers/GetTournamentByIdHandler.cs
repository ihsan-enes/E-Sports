using ESpor.Application.Common.Interfaces;
using ESpor.Application.Tournaments.DTOs;
using Microsoft.EntityFrameworkCore;

namespace ESpor.Application.Tournaments.Handlers;

public class GetTournamentByIdHandler
{
    private readonly IApplicationDbContext _context;

    public GetTournamentByIdHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TournamentDetailDto?> HandleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var nowUtc = DateTimeOffset.UtcNow;

        return await _context.Tournaments
            .AsNoTracking()
            .Where(t => t.Id == id)
            .Select(t => new TournamentDetailDto(
                t.Id,
                t.ExternalId,
                new GameSummaryDto(t.Game.Id, t.Game.Name, t.Game.Slug, t.Game.IconUrl),
                t.Name,
                t.Slug,
                t.Tier,
                t.Region,
                t.Country,
                t.LeagueSlug,
                t.StartDateUtc,
                t.EndDateUtc,
                t.EndDateUtc != null && t.EndDateUtc < nowUtc ? "finished" :
                t.StartDateUtc != null && t.StartDateUtc > nowUtc ? "upcoming" : "running",
                t.Matches.Count,
                t.CreatedAtUtc,
                t.UpdatedAtUtc
            ))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
