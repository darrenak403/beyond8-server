using Beyond8.Common;
using Beyond8.Sale.Application.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Beyond8.Sale.Application.Services.Implements;

public class SettlementBackgroundService : BackgroundService
{
    private readonly ILogger<SettlementBackgroundService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public SettlementBackgroundService(ILogger<SettlementBackgroundService> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("SettlementBackgroundService started");

        // Simple loop: run daily at ~02:00 UTC. For simplicity run every hour and let service decide eligible items.
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var settlementService = scope.ServiceProvider.GetRequiredService<ISettlementService>();
                await settlementService.ProcessPendingSettlementsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SettlementBackgroundService failed to process settlements");
            }

            // Wait an hour before next run
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }
}
