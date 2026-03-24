using FluentAssertions;
using Xunit;

namespace Beyond8.IntegrationTests.Flows;

public class StudentCoursePurchaseTests
{
    [Fact]
    public void PurchasePipeline_ShouldCompleteHappyPath_FromCartToSuccessfulPayment()
    {
        var courseId = Guid.NewGuid();
        var state = PurchaseFlowState.Begin(courseId);

        state.AddToCart();
        state.IsInCart.Should().BeTrue();

        state.BookFromCart();
        state.BookingId.Should().NotBeEmpty();
        state.BookingStatus.Should().Be(BookingStatus.Reserved);

        state.BuyFromBooking();
        state.CourseId.Should().Be(courseId);
        state.OrderId.Should().NotBeEmpty();
        state.OrderStatus.Should().Be(OrderStatus.PendingPayment);
        state.PaymentUrl.Should().NotBeNullOrWhiteSpace();
        state.ProcessCallback(isSuccess: true);
        state.OrderStatus.Should().Be(OrderStatus.Paid);
        state.PaymentId.Should().NotBeEmpty();
        state.PaymentStatus.Should().Be(PaymentStatus.Success);
        state.GetPurchasedCourseIds().Should().Contain(courseId);
    }

    [Fact]
    public void PurchasePipeline_ShouldFailOrder_WhenPaymentCallbackIsFailed()
    {
        var state = PurchaseFlowState.Begin(Guid.NewGuid());
        state.AddToCart();
        state.BookFromCart();
        state.BuyFromBooking();

        state.ProcessCallback(isSuccess: false);

        state.OrderStatus.Should().Be(OrderStatus.Failed);
        state.PaymentStatus.Should().Be(PaymentStatus.Failed);
        state.GetPurchasedCourseIds().Should().BeEmpty();
    }

    [Fact]
    public void PurchasePipeline_ShouldKeepIdempotency_WhenPaymentSuccessCallbackIsDuplicated()
    {
        var state = PurchaseFlowState.Begin(Guid.NewGuid());
        state.AddToCart();
        state.BookFromCart();
        state.BuyFromBooking();

        state.ProcessCallback(isSuccess: true);
        var firstPaymentId = state.PaymentId;
        var firstPaidCount = state.PaidTransactionCount;

        state.ProcessCallback(isSuccess: true);

        state.PaymentId.Should().Be(firstPaymentId, "duplicate callback must be idempotent");
        state.PaidTransactionCount.Should().Be(firstPaidCount, "must not create duplicate settlement");
    }

    [Fact]
    public void PurchasePipeline_ShouldRejectBooking_WhenCourseNotInCart()
    {
        var state = PurchaseFlowState.Begin(Guid.NewGuid());

        var act = () => state.BookFromCart();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void PurchasePipeline_ShouldRejectBuy_WhenBookingDoesNotExist()
    {
        var state = PurchaseFlowState.Begin(Guid.NewGuid());
        state.AddToCart();

        var act = () => state.BuyFromBooking();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void PurchasePipeline_ShouldRejectDuplicateBooking_ForSameCartItem()
    {
        var state = PurchaseFlowState.Begin(Guid.NewGuid());
        state.AddToCart();
        state.BookFromCart();

        var act = () => state.BookFromCart();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void PurchasePipeline_ShouldKeepPurchasedCourseListEmpty_BeforePaymentSuccess()
    {
        var courseId = Guid.NewGuid();
        var state = PurchaseFlowState.Begin(courseId);
        state.AddToCart();
        state.BookFromCart();
        state.BuyFromBooking();

        state.GetPurchasedCourseIds().Should().BeEmpty();
    }

    private enum OrderStatus
    {
        PendingPayment,
        Paid,
        Failed
    }

    private enum PaymentStatus
    {
        Pending,
        Success,
        Failed
    }

    private enum BookingStatus
    {
        None,
        Reserved
    }

    private sealed class PurchaseFlowState
    {
        public Guid CourseId { get; private set; }
        public bool IsInCart { get; private set; }
        public Guid BookingId { get; private set; }
        public BookingStatus BookingStatus { get; private set; }
        public Guid OrderId { get; private set; }
        public Guid PaymentId { get; private set; }
        public string PaymentUrl { get; private set; } = string.Empty;
        public OrderStatus OrderStatus { get; private set; }
        public PaymentStatus PaymentStatus { get; private set; }
        public int PaidTransactionCount { get; private set; }

        public static PurchaseFlowState Begin(Guid courseId)
            => new()
            {
                CourseId = courseId,
                BookingStatus = BookingStatus.None
            };

        public void AddToCart() => IsInCart = true;

        public void BookFromCart()
        {
            if (!IsInCart)
            {
                throw new InvalidOperationException("Course must be in cart before booking.");
            }
            if (BookingStatus == BookingStatus.Reserved)
            {
                throw new InvalidOperationException("Booking already exists for this cart item.");
            }

            BookingId = Guid.NewGuid();
            BookingStatus = BookingStatus.Reserved;
        }

        public void BuyFromBooking()
        {
            if (BookingStatus != BookingStatus.Reserved || BookingId == Guid.Empty)
            {
                throw new InvalidOperationException("Booking must exist before buy.");
            }

            OrderId = Guid.NewGuid();
            PaymentUrl = $"https://pay.mock/vnpay?orderId={OrderId}";
            OrderStatus = OrderStatus.PendingPayment;
            PaymentStatus = PaymentStatus.Pending;
        }

        public void ProcessCallback(bool isSuccess)
        {
            if (isSuccess)
            {
                if (OrderStatus == OrderStatus.Paid)
                {
                    return;
                }

                OrderStatus = OrderStatus.Paid;
                PaymentStatus = PaymentStatus.Success;
                PaymentId = Guid.NewGuid();
                PaidTransactionCount++;
                return;
            }

            OrderStatus = OrderStatus.Failed;
            PaymentStatus = PaymentStatus.Failed;
        }

        public IReadOnlyList<Guid> GetPurchasedCourseIds()
            => OrderStatus == OrderStatus.Paid ? [CourseId] : [];
    }
}
