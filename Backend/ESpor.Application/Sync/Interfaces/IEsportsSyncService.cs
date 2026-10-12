using ESpor.Application.Common;

namespace ESpor.Application.Sync.Interfaces;

public interface IEsportsSyncService
{
    Task<Result> SyncLiveMatchesAsync(CancellationToken cancellationToken = default);
    Task<Result> SyncFixturesAsync(CancellationToken cancellationToken = default);
    Task<Result> SyncStatsAsync(CancellationToken cancellationToken = default);
}
