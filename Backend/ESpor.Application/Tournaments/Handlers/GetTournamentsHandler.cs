using ESpor.Application.Common.Helpers;
using ESpor.Application.Common.Interfaces;
using ESpor.Application.Common.Models;
using ESpor.Application.Tournaments.DTOs;
using ESpor.Application.Tournaments.Queries;
using Microsoft.EntityFrameworkCore;

namespace ESpor.Application.Tournaments.Handlers;

public class GetTournamentsHandler
{
    private readonly IApplicationDbContext _context;

    public GetTournamentsHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<TournamentDto>> HandleAsync(GetTournamentsQuery query, CancellationToken cancellationToken = default)
    {
        var normalizedGameSlug = GameSlugHelper.NormalizeGameSlug(query.Game);
        var nowUtc = DateTimeOffset.UtcNow;

        var tournamentsQuery = _context.Tournaments
            .AsNoTracking();

        if (!string.IsNullOrEmpty(normalizedGameSlug))
        {
            tournamentsQuery = tournamentsQuery.Where(t => t.Game.Slug == normalizedGameSlug);
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            var statusNormalized = query.Status.Trim().ToLowerInvariant();
            switch (statusNormalized)
            {
                case "upcoming":
                    tournamentsQuery = tournamentsQuery.Where(t => t.StartDateUtc > nowUtc);
                    break;
                case "running" or "live":
                    tournamentsQuery = tournamentsQuery.Where(t =>
                        (t.StartDateUtc == null || t.StartDateUtc <= nowUtc) &&
                        (t.EndDateUtc == null || t.EndDateUtc >= nowUtc));
                    break;
                case "finished" or "past":
                    tournamentsQuery = tournamentsQuery.Where(t => t.EndDateUtc != null && t.EndDateUtc < nowUtc);
                    break;
            }
        }

        var totalCount = await tournamentsQuery.CountAsync(cancellationToken);

        var skip = (query.Page - 1) * query.PageSize;
        var items = await tournamentsQuery
            .OrderByDescending(t => t.StartDateUtc ?? t.CreatedAtUtc)
            .Skip(skip)
            .Take(query.PageSize)
            .Select(t => new TournamentDto(
                t.Id,
                t.ExternalId,
                t.Game.Slug,
                t.Game.Name,
                t.Name,
                t.Slug,
                t.Tier,
                t.Region,
                t.Country,
                t.LeagueSlug,
                t.StartDateUtc,
                t.EndDateUtc,
                t.EndDateUtc != null && t.EndDateUtc < nowUtc ? "finished" :
                t.StartDateUtc != null && t.StartDateUtc > nowUtc ? "upcoming" : "running"
            ))
            .ToListAsync(cancellationToken);

        return new PagedResult<TournamentDto>(items, query.Page, query.PageSize, totalCount);
    }
}
