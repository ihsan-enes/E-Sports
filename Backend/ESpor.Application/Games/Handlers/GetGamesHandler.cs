using ESpor.Application.Common.Interfaces;
using ESpor.Application.Games.DTOs;
using Microsoft.EntityFrameworkCore;

namespace ESpor.Application.Games.Handlers;

public class GetGamesHandler
{
    private readonly IApplicationDbContext _context;

    public GetGamesHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<GameDto>> HandleAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Games
            .AsNoTracking()
            .OrderBy(g => g.Name)
            .Select(g => new GameDto(g.Id, g.Name, g.Slug, g.IconUrl))
            .ToListAsync(cancellationToken);
    }
}
