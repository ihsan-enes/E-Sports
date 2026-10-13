using ESpor.Application.Common;
using ESpor.Application.Common.Options;
using ESpor.Application.Sync.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ESpor.Infrastructure.Sync.Jobs;

public class StatsSyncJob : BaseSyncJob
{
    private readonly IOptions<SyncOptions> _options;

    public StatsSyncJob(
        IServiceScopeFactory scopeFactory,
        IOptions<SyncOptions> options,
        ILogger<StatsSyncJob> logger)
        : base(scopeFactory, logger, "stats")
    {
        _options = options;
    }

    protected override TimeSpan GetInterval()
    {
        var days = _options.Value.StatsIntervalDays > 0 ? _options.Value.StatsIntervalDays : 7;
        return TimeSpan.FromDays(days);
    }

    protected override Task<Result> ExecuteSyncAsync(IEsportsSyncService syncService, CancellationToken cancellationToken)
    {
        return syncService.SyncStatsAsync(cancellationToken);
    }
}
