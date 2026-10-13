using ESpor.Application.Common.Interfaces;
using ESpor.Application.Matches.DTOs;
using ESpor.Application.Players.DTOs;
using ESpor.Application.Tournaments.DTOs;
using Microsoft.EntityFrameworkCore;

namespace ESpor.Application.Players.Handlers;

public class GetPlayerByIdHandler
{
    private readonly IApplicationDbContext _context;

    public GetPlayerByIdHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PlayerDetailDto?> HandleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Players
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new PlayerDetailDto(
                p.Id,
                p.ExternalId,
                p.Team == null ? null : new TeamSummaryDto(
                    p.Team.Id,
                    p.Team.Name,
                    p.Team.Slug,
                    p.Team.Acronym,
                    p.Team.Country,
                    p.Team.LogoUrl
                ),
                p.Team != null && p.Team.Game != null
                    ? new GameSummaryDto(p.Team.Game.Id, p.Team.Game.Name, p.Team.Game.Slug, p.Team.Game.IconUrl)
                    : null,
                p.Nickname,
                p.FirstName,
                p.LastName,
                p.Role,
                p.Country,
                p.ImageUrl,
                p.CreatedAtUtc,
                p.UpdatedAtUtc
            ))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
