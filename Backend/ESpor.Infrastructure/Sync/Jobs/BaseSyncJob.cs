using ESpor.Application.Common;
using ESpor.Application.Common.Interfaces;
using ESpor.Application.Sync.Interfaces;
using ESpor.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ESpor.Infrastructure.Sync.Jobs;

public abstract class BaseSyncJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger _logger;
    private readonly string _jobName;

    protected BaseSyncJob(IServiceScopeFactory scopeFactory, ILogger logger, string jobName)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _jobName = jobName;
    }

    protected abstract TimeSpan GetInterval();
    protected abstract Task<Result> ExecuteSyncAsync(IEsportsSyncService syncService, CancellationToken cancellationToken);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("{JobName} servisi başlatıldı.", _jobName);

        var interval = GetInterval();

        // 1. Başlangıçta gecikme (overdue) kontrolü
        await CheckAndRunIfOverdueAsync(interval, stoppingToken);

        // 2. PeriodicTimer ile periyodik döngü
        using var timer = new PeriodicTimer(interval);
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    await RunJobAsync(stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{JobName} periyodik döngü sırasında beklenmeyen bir hata ile karşılaştı.", _jobName);
            }
        }

        _logger.LogInformation("{JobName} servisi durduruluyor.", _jobName);
    }

    private async Task CheckAndRunIfOverdueAsync(TimeSpan interval, CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

            var syncState = await context.SyncStates
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.JobName == _jobName, cancellationToken);

            var now = DateTimeOffset.UtcNow;
            var isOverdue = syncState?.LastSuccessUtc == null || (now - syncState.LastSuccessUtc.Value) >= interval;

            if (isOverdue)
            {
                _logger.LogInformation("{JobName} son başarılı çalışmadan bu yana aralık aşıldığı veya hiç çalışmadığı için hemen yürütülüyor.", _jobName);
                await RunJobAsync(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{JobName} başlangıç kontrolü sırasında hata oluştu.", _jobName);
        }
    }

    private async Task RunJobAsync(CancellationToken cancellationToken)
    {
        var attemptUtc = DateTimeOffset.UtcNow;
        _logger.LogInformation("{JobName} yürütülüyor...", _jobName);

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
            var syncService = scope.ServiceProvider.GetRequiredService<IEsportsSyncService>();

            var syncState = await context.SyncStates.FirstOrDefaultAsync(s => s.JobName == _jobName, cancellationToken);
            if (syncState == null)
            {
                syncState = new SyncState
                {
                    JobName = _jobName,
                    LastAttemptUtc = attemptUtc
                };
                context.SyncStates.Add(syncState);
            }
            else
            {
                syncState.LastAttemptUtc = attemptUtc;
            }

            var result = await ExecuteSyncAsync(syncService, cancellationToken);

            if (result.IsSuccess)
            {
                syncState.LastSuccessUtc = DateTimeOffset.UtcNow;
                syncState.LastError = null;
                _logger.LogInformation("{JobName} başarıyla tamamlandı.", _jobName);
            }
            else
            {
                syncState.LastError = result.Error?.Length > 2000 ? result.Error[..2000] : result.Error;
                _logger.LogWarning("{JobName} başarısız oldu: {Error}", _jobName, result.Error);
            }

            await context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{JobName} çalıştırılırken yakalanmamış bir hata oluştu.", _jobName);

            try
            {
                using var errorScope = _scopeFactory.CreateScope();
                var errorContext = errorScope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
                var errorState = await errorContext.SyncStates.FirstOrDefaultAsync(s => s.JobName == _jobName, cancellationToken);

                if (errorState == null)
                {
                    errorState = new SyncState
                    {
                        JobName = _jobName,
                        LastAttemptUtc = attemptUtc,
                        LastError = ex.Message.Length > 2000 ? ex.Message[..2000] : ex.Message
                    };
                    errorContext.SyncStates.Add(errorState);
                }
                else
                {
                    errorState.LastAttemptUtc = attemptUtc;
                    errorState.LastError = ex.Message.Length > 2000 ? ex.Message[..2000] : ex.Message;
                }

                await errorContext.SaveChangesAsync(cancellationToken);
            }
            catch (Exception recordEx)
            {
                _logger.LogError(recordEx, "{JobName} hata durumunu veritabanına kaydederken ikinci bir hata oluştu.", _jobName);
            }
        }
    }
}
