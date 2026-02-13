using Beyond8.Common.Data.Interfaces;

namespace Beyond8.Sale.Domain.Repositories.Interfaces;

public interface IUnitOfWork : IBaseUnitOfWork
{
    IOrderRepository OrderRepository { get; }
    IOrderItemRepository OrderItemRepository { get; }
    IPaymentRepository PaymentRepository { get; }
    ICouponRepository CouponRepository { get; }
    ICouponUsageRepository CouponUsageRepository { get; }
    IInstructorWalletRepository InstructorWalletRepository { get; }
    IPayoutRequestRepository PayoutRequestRepository { get; }
    ITransactionLedgerRepository TransactionLedgerRepository { get; }
    ICartRepository CartRepository { get; }
    ICartItemRepository CartItemRepository { get; }
    IPlatformWalletRepository PlatformWalletRepository { get; }
    IPlatformWalletTransactionRepository PlatformWalletTransactionRepository { get; }
}
