using ESpor.Application.Common.Interfaces;
using ESpor.Application.Matches.DTOs;
using ESpor.Application.Teams.DTOs;
using ESpor.Application.Tournaments.DTOs;
using ESpor.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ESpor.Application.Teams.Handlers;

public class GetTeamByIdHandler
{
    private readonly IApplicationDbContext _context;

    public GetTeamByIdHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TeamDetailDto?> HandleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var team = await _context.Teams
            .AsNoTracking()
            .Include(t => t.Game)
            .Include(t => t.Players)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (team == null)
        {
            return null;
        }

        // Biten tüm maçlar (Win rate hesabı için)
        var finishedMatches = await _context.Matches
            .AsNoTracking()
            .Where(m => (m.Team1Id == id || m.Team2Id == id) && m.Status == MatchStatus.Finished)
            .OrderByDescending(m => m.EndedAtUtc ?? m.ScheduledAtUtc)
            .Select(m => new
            {
                Match = m,
                IsWinner = m.WinnerTeamId == id
            })
            .ToListAsync(cancellationToken);

        var totalMatchesPlayed = finishedMatches.Count;
        var totalWins = finishedMatches.Count(m => m.IsWinner);
        var winRate = totalMatchesPlayed > 0
            ? Math.Round(((double)totalWins / totalMatchesPlayed) * 100, 2)
            : 0.0;

        // Form (son 5 maçın W/L durumu)
        var form = finishedMatches
            .Take(5)
            .Select(m => m.IsWinner ? "W" : "L")
            .ToList();

        // Son 10 bitmiş maç
        var recentMatches = await _context.Matches
            .AsNoTracking()
            .Where(m => (m.Team1Id == id || m.Team2Id == id) && m.Status == MatchStatus.Finished)
            .OrderByDescending(m => m.EndedAtUtc ?? m.ScheduledAtUtc)
            .Take(10)
            .Select(m => new MatchDto(
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
                m.WinnerTeamId
            ))
            .ToListAsync(cancellationToken);

        var roster = team.Players
            .OrderBy(p => p.Nickname)
            .Select(p => new PlayerSummaryDto(
                p.Id,
                p.ExternalId,
                p.Nickname,
                p.FirstName,
                p.LastName,
                p.Role,
                p.Country,
                p.ImageUrl
            ))
            .ToList();

        return new TeamDetailDto(
            team.Id,
            team.ExternalId,
            team.Game == null ? null : new GameSummaryDto(team.Game.Id, team.Game.Name, team.Game.Slug, team.Game.IconUrl),
            team.Name,
            team.Slug,
            team.Acronym,
            team.Country,
            team.LogoUrl,
            totalMatchesPlayed,
            totalWins,
            winRate,
            form,
            roster,
            recentMatches,
            team.CreatedAtUtc,
            team.UpdatedAtUtc
        );
    }
}
