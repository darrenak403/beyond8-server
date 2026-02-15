using Beyond8.Sale.Application.Services.Interfaces;
using Beyond8.Sale.Domain.Entities;
using Beyond8.Sale.Domain.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace Beyond8.Sale.Application.Services.Implements;

public class PaymentCleanupService : BackgroundService, IPaymentCleanupService
{
    private readonly ILogger<PaymentCleanupService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(5); // Run every 5 minutes

    public PaymentCleanupService(
        ILogger<PaymentCleanupService> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("PaymentCleanupService started. Running every {Interval} minutes.", _cleanupInterval.TotalMinutes);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupExpiredPaymentsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while cleaning up expired payments");
            }

            await Task.Delay(_cleanupInterval, stoppingToken);
        }

        _logger.LogInformation("PaymentCleanupService stopped.");
    }

    private async Task CleanupExpiredPaymentsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var expiredPayments = await unitOfWork.PaymentRepository.AsQueryable()
            .Where(p => p.Status == Domain.Enums.PaymentStatus.Pending
                     && p.ExpiredAt < DateTime.UtcNow)
            .ToListAsync(cancellationToken);

        if (!expiredPayments.Any())
        {
            _logger.LogDebug("No expired payments found to clean up.");
            return;
        }

        _logger.LogInformation("Found {Count} expired payments to clean up.", expiredPayments.Count);

        foreach (var payment in expiredPayments)
        {
            payment.Status = Domain.Enums.PaymentStatus.Expired;
            payment.FailureReason = "Payment expired after 15 minutes";
            payment.UpdatedAt = DateTime.UtcNow;

            // If payment has an order, update order status back to Pending (user can retry)
            if (payment.OrderId.HasValue)
            {
                var order = await unitOfWork.OrderRepository.FindOneAsync(o => o.Id == payment.OrderId.Value);
                if (order != null && order.Status == Domain.Enums.OrderStatus.Pending)
                {
                    // Order stays Pending so user can create new payment
                    _logger.LogInformation("Order {OrderId} remains pending for retry after payment {PaymentId} expired.",
                        order.Id, payment.Id);
                }
            }

            _logger.LogInformation("Marked payment {PaymentId} as expired.", payment.Id);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully cleaned up {Count} expired payments.", expiredPayments.Count);
    }
}