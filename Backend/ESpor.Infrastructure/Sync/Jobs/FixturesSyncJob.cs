using ESpor.Application.Common;
using ESpor.Application.Common.Options;
using ESpor.Application.Sync.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ESpor.Infrastructure.Sync.Jobs;

public class FixturesSyncJob : BaseSyncJob
{
    private readonly IOptions<SyncOptions> _options;

    public FixturesSyncJob(
        IServiceScopeFactory scopeFactory,
        IOptions<SyncOptions> options,
        ILogger<FixturesSyncJob> logger)
        : base(scopeFactory, logger, "fixtures")
    {
        _options = options;
    }

    protected override TimeSpan GetInterval()
    {
        var hours = _options.Value.FixturesIntervalHours > 0 ? _options.Value.FixturesIntervalHours : 24;
        return TimeSpan.FromHours(hours);
    }

    protected override Task<Result> ExecuteSyncAsync(IEsportsSyncService syncService, CancellationToken cancellationToken)
    {
        return syncService.SyncFixturesAsync(cancellationToken);
    }
}
