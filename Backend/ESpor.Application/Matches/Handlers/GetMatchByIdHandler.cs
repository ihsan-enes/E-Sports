using ESpor.Application.Common.Interfaces;
using ESpor.Application.Matches.DTOs;
using Microsoft.EntityFrameworkCore;

namespace ESpor.Application.Matches.Handlers;

public class GetMatchByIdHandler
{
    private readonly IApplicationDbContext _context;

    public GetMatchByIdHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<MatchDetailDto?> HandleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Matches
            .AsNoTracking()
            .Where(m => m.Id == id)
            .Select(m => new MatchDetailDto(
                m.Id,
                m.ExternalId,
                m.Game.Slug,
                m.Game.Name,
                new TournamentSummaryDto(
                    m.Tournament.Id,
                    m.Tournament.Name,
                    m.Tournament.Slug,
                    m.Tournament.Tier,
                    m.Tournament.LeagueSlug
                ),
                m.Status,
                m.ScheduledAtUtc,
                m.StartedAtUtc,
                m.EndedAtUtc,
                m.BestOf,
                m.Team1 == null ? null : new TeamSummaryDto(
                    m.Team1.Id,
                    m.Team1.Name,
                    m.Team1.Slug,
                    m.Team1.Acronym,
                    m.Team1.Country,
                    m.Team1.LogoUrl
                ),
                m.Team2 == null ? null : new TeamSummaryDto(
                    m.Team2.Id,
                    m.Team2.Name,
                    m.Team2.Slug,
                    m.Team2.Acronym,
                    m.Team2.Country,
                    m.Team2.LogoUrl
                ),
                m.Team1Score,
                m.Team2Score,
                m.WinnerTeamId,
                m.CreatedAtUtc,
                m.UpdatedAtUtc
            ))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
