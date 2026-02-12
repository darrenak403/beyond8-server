using Beyond8.Sale.Domain.Repositories.Interfaces;
using Beyond8.Sale.Infrastructure.Data;
using Beyond8.Common.Data.Implements;

namespace Beyond8.Sale.Infrastructure.Repositories.Implements;

public class UnitOfWork(SaleDbContext context) : BaseUnitOfWork<SaleDbContext>(context), IUnitOfWork
{
    private IOrderRepository? _orderRepository;
    private IOrderItemRepository? _orderItemRepository;
    private IPaymentRepository? _paymentRepository;
    private ICouponRepository? _couponRepository;
    private ICouponUsageRepository? _couponUsageRepository;
    private IInstructorWalletRepository? _instructorWalletRepository;
    private IPayoutRequestRepository? _payoutRequestRepository;
    private ITransactionLedgerRepository? _transactionLedgerRepository;
    private ICartRepository? _cartRepository;
    private ICartItemRepository? _cartItemRepository;
    private IPlatformWalletRepository? _platformWalletRepository;

    public IOrderRepository OrderRepository => _orderRepository ??= new OrderRepository(context);
    public IOrderItemRepository OrderItemRepository => _orderItemRepository ??= new OrderItemRepository(context);
    public IPaymentRepository PaymentRepository => _paymentRepository ??= new PaymentRepository(context);
    public ICouponRepository CouponRepository => _couponRepository ??= new CouponRepository(context);
    public ICouponUsageRepository CouponUsageRepository => _couponUsageRepository ??= new CouponUsageRepository(context);
    public IInstructorWalletRepository InstructorWalletRepository => _instructorWalletRepository ??= new InstructorWalletRepository(context);
    public IPayoutRequestRepository PayoutRequestRepository => _payoutRequestRepository ??= new PayoutRequestRepository(context);
    public ITransactionLedgerRepository TransactionLedgerRepository => _transactionLedgerRepository ??= new TransactionLedgerRepository(context);
    public ICartRepository CartRepository => _cartRepository ??= new CartRepository(context);
    public ICartItemRepository CartItemRepository => _cartItemRepository ??= new CartItemRepository(context);
    public IPlatformWalletRepository PlatformWalletRepository => _platformWalletRepository ??= new PlatformWalletRepository(context);
}
