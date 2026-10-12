using ESpor.Application.Common;
using ESpor.Application.Common.Interfaces;
using ESpor.Application.Sync.Interfaces;
using ESpor.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ESpor.Application.Sync.Handlers;

public class TriggerSyncHandler
{
    private readonly IEsportsSyncService _syncService;
    private readonly IApplicationDbContext _context;

    public TriggerSyncHandler(IEsportsSyncService syncService, IApplicationDbContext context)
    {
        _syncService = syncService;
        _context = context;
    }

    public async Task<Result<string>> HandleAsync(string job, CancellationToken cancellationToken = default)
    {
        var jobNormalized = job.Trim().ToLowerInvariant();
        var now = DateTimeOffset.UtcNow;

        var syncState = await _context.SyncStates.FirstOrDefaultAsync(s => s.JobName == jobNormalized, cancellationToken);
        if (syncState == null)
        {
            syncState = new SyncState
            {
                JobName = jobNormalized,
                LastAttemptUtc = now
            };
            _context.SyncStates.Add(syncState);
        }
        else
        {
            syncState.LastAttemptUtc = now;
        }

        Result result = jobNormalized switch
        {
            "live" => await _syncService.SyncLiveMatchesAsync(cancellationToken),
            "fixtures" => await _syncService.SyncFixturesAsync(cancellationToken),
            "stats" => await _syncService.SyncStatsAsync(cancellationToken),
            "all" => await RunAllAsync(cancellationToken),
            _ => Result.Failure($"Bilinmeyen senkronizasyon görevi: '{job}'. Geçerli değerler: 'live', 'fixtures', 'stats', 'all'")
        };

        if (result.IsSuccess)
        {
            syncState.LastSuccessUtc = DateTimeOffset.UtcNow;
            syncState.LastError = null;
            await _context.SaveChangesAsync(cancellationToken);
            return Result<string>.Success($"'{jobNormalized}' senkronizasyonu başarıyla tamamlandı.");
        }

        syncState.LastError = result.Error?.Length > 2000 ? result.Error[..2000] : result.Error;
        await _context.SaveChangesAsync(cancellationToken);
        return Result<string>.Failure(result.Error ?? "Senkronizasyon başarısız oldu.");
    }

    private async Task<Result> RunAllAsync(CancellationToken cancellationToken)
    {
        var liveResult = await _syncService.SyncLiveMatchesAsync(cancellationToken);
        if (!liveResult.IsSuccess) return liveResult;

        var fixturesResult = await _syncService.SyncFixturesAsync(cancellationToken);
        if (!fixturesResult.IsSuccess) return fixturesResult;

        var statsResult = await _syncService.SyncStatsAsync(cancellationToken);
        if (!statsResult.IsSuccess) return statsResult;

        return Result.Success();
    }
}
