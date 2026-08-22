using AslanEtsy.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AslanEtsy.Infrastructure.BackgroundServices;

public class EtsyOrderSyncBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EtsyOrderSyncBackgroundService> _logger;
    private readonly TimeSpan _syncInterval = TimeSpan.FromMinutes(15); // Sync every 15 minutes

    public EtsyOrderSyncBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<EtsyOrderSyncBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Etsy Otomatik Sipariş Senkronizasyon Arka Plan Servisi Başlatıldı.");

        // Initial delay on startup
        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Periyodik Etsy sipariş senkronizasyonu başlatılıyor: {Time}", DateTimeOffset.UtcNow);

                using (var scope = _serviceProvider.CreateScope())
                {
                    var syncService = scope.ServiceProvider.GetRequiredService<IEtsySyncService>();
                    var results = await syncService.SyncAllActiveAccountsAsync(stoppingToken);

                    foreach (var res in results)
                    {
                        if (res.Status == Domain.Enums.SyncStatus.Success)
                        {
                            _logger.LogInformation("Mağaza {Shop} senkronize edildi: {Fetched} sipariş çekildi, {Created} yeni, {Updated} güncellendi.",
                                res.ShopName, res.OrdersFetched, res.OrdersCreated, res.OrdersUpdated);
                        }
                        else
                        {
                            _logger.LogWarning("Mağaza {Shop} senkronizasyonunda hata: {Error}", res.ShopName, res.ErrorMessage);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Arka plan Etsy senkronizasyonunda beklenmedik bir hata oluştu.");
            }

            try
            {
                await Task.Delay(_syncInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        _logger.LogInformation("Etsy Otomatik Sipariş Senkronizasyon Servisi Durduruldu.");
    }
}
