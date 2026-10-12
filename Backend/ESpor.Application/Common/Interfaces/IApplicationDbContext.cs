using ESpor.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ESpor.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Game> Games { get; }
    DbSet<Tournament> Tournaments { get; }
    DbSet<Team> Teams { get; }
    DbSet<Player> Players { get; }
    DbSet<Match> Matches { get; }
    DbSet<SyncState> SyncStates { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
