using ESpor.Application.Common.Helpers;
using ESpor.Application.Common.Interfaces;
using ESpor.Application.Common.Models;
using ESpor.Application.Teams.DTOs;
using ESpor.Application.Teams.Queries;
using ESpor.Application.Tournaments.DTOs;
using Microsoft.EntityFrameworkCore;

namespace ESpor.Application.Teams.Handlers;

public class GetTeamsHandler
{
    private readonly IApplicationDbContext _context;

    public GetTeamsHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<TeamDto>> HandleAsync(GetTeamsQuery query, CancellationToken cancellationToken = default)
    {
        var normalizedGameSlug = GameSlugHelper.NormalizeGameSlug(query.Game);

        var teamsQuery = _context.Teams
            .AsNoTracking();

        if (!string.IsNullOrEmpty(normalizedGameSlug))
        {
            teamsQuery = teamsQuery.Where(t => t.Game != null && t.Game.Slug == normalizedGameSlug);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            teamsQuery = teamsQuery.Where(t =>
                EF.Functions.Like(t.Name.ToLower(), $"%{search}%") ||
                EF.Functions.Like(t.Slug.ToLower(), $"%{search}%") ||
                (t.Acronym != null && EF.Functions.Like(t.Acronym.ToLower(), $"%{search}%")));
        }

        var totalCount = await teamsQuery.CountAsync(cancellationToken);

        var skip = (query.Page - 1) * query.PageSize;
        var items = await teamsQuery
            .OrderBy(t => t.Name)
            .Skip(skip)
            .Take(query.PageSize)
            .Select(t => new TeamDto(
                t.Id,
                t.ExternalId,
                t.Game == null ? null : new GameSummaryDto(t.Game.Id, t.Game.Name, t.Game.Slug, t.Game.IconUrl),
                t.Name,
                t.Slug,
                t.Acronym,
                t.Country,
                t.LogoUrl
            ))
            .ToListAsync(cancellationToken);

        return new PagedResult<TeamDto>(items, query.Page, query.PageSize, totalCount);
    }
}
