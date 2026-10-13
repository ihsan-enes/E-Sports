using ESpor.Application.Common;
using ESpor.Application.Common.Options;
using ESpor.Application.Sync.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ESpor.Infrastructure.Sync.Jobs;

public class LiveSyncJob : BaseSyncJob
{
    private readonly IOptions<SyncOptions> _options;

    public LiveSyncJob(
        IServiceScopeFactory scopeFactory,
        IOptions<SyncOptions> options,
        ILogger<LiveSyncJob> logger)
        : base(scopeFactory, logger, "live")
    {
        _options = options;
    }

    protected override TimeSpan GetInterval()
    {
        var seconds = _options.Value.LiveIntervalSeconds > 0 ? _options.Value.LiveIntervalSeconds : 60;
        return TimeSpan.FromSeconds(seconds);
    }

    protected override Task<Result> ExecuteSyncAsync(IEsportsSyncService syncService, CancellationToken cancellationToken)
    {
        return syncService.SyncLiveMatchesAsync(cancellationToken);
    }
}
