using Beyond8.Common;
using Beyond8.Common.Events.Cache;
using Beyond8.Common.Events.Sale;
using Beyond8.Common.Utilities;
using Beyond8.Sale.Application.Dtos.Payments;
using Beyond8.Sale.Application.Dtos.VNPays;
using Beyond8.Sale.Application.Helpers;
using Beyond8.Sale.Application.Mappings.Payments;
using Beyond8.Sale.Application.Services.Interfaces;
using Beyond8.Sale.Domain.Entities;
using Beyond8.Sale.Domain.Enums;
using Beyond8.Sale.Domain.Repositories.Interfaces;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Beyond8.Sale.Application.Services.Implements;

public class PaymentService(
    ILogger<PaymentService> logger,
    IUnitOfWork unitOfWork,
    IVNPayService vnPayService,
    IInstructorWalletService walletService,
    IPlatformWalletService platformWalletService,
    ICouponUsageService couponUsageService,
    IPublishEndpoint publishEndpoint) : IPaymentService
{
    public async Task<ApiResponse<PaymentUrlResponse>> ProcessPaymentAsync(
        Guid orderId, string returnUrl, string ipAddress)
    {
        var order = await unitOfWork.OrderRepository.AsQueryable()
            .Include(o => o.OrderItems)
            .Include(o => o.Payments)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
            return ApiResponse<PaymentUrlResponse>.FailureResponse("Đơn hàng không tồn tại");

        if (order.Status != OrderStatus.Pending)
            return ApiResponse<PaymentUrlResponse>.FailureResponse("Đơn hàng không ở trạng thái chờ thanh toán");

        if (order.TotalAmount <= 0)
            return ApiResponse<PaymentUrlResponse>.FailureResponse("Đơn hàng miễn phí không cần thanh toán");

        // Reuse existing pending payment if still valid
        var existingPayment = order.Payments.FirstOrDefault(
            p => p.Status is PaymentStatus.Pending or PaymentStatus.Processing
            && p.ExpiredAt > DateTime.UtcNow);
        if (existingPayment != null)
        {
            // If payment URL already exists, return it without regenerating
            if (!string.IsNullOrEmpty(existingPayment.PaymentUrl))
            {
                return ApiResponse<PaymentUrlResponse>.SuccessResponse(
                    existingPayment.ToUrlResponse(existingPayment.PaymentUrl),
                    "Đã có giao dịch thanh toán đang chờ xử lý");
            }

            // Generate URL only if not already stored
            var existingUrl = GenerateVNPayUrl(existingPayment, order, ipAddress, returnUrl);
            existingPayment.PaymentUrl = existingUrl;
            await unitOfWork.SaveChangesAsync();

            return ApiResponse<PaymentUrlResponse>.SuccessResponse(
                existingPayment.ToUrlResponse(existingUrl),
                "Đã có giao dịch thanh toán đang chờ xử lý");
        }

        var payment = new Payment
        {
            OrderId = orderId,
            PaymentNumber = GeneratePaymentNumber(),
            Purpose = PaymentPurpose.OrderPayment,
            Status = PaymentStatus.Pending,
            Amount = order.TotalAmount,
            Currency = order.Currency,
            Provider = "VNPay",
            PaymentMethod = "VNPay",
            ExpiredAt = DateTime.UtcNow.AddMinutes(15),
            CreatedAt = DateTime.UtcNow
        };

        await unitOfWork.PaymentRepository.AddAsync(payment);
        await unitOfWork.SaveChangesAsync();

        var paymentUrl = GenerateVNPayUrl(payment, order, ipAddress, returnUrl);

        // Save payment URL for later retrieval
        payment.PaymentUrl = paymentUrl;
        await unitOfWork.SaveChangesAsync();

        logger.LogInformation(
            "Payment initiated — PaymentNumber: {PaymentNumber}, OrderId: {OrderId}, Amount: {Amount}",
            payment.PaymentNumber, orderId, order.TotalAmount);

        return ApiResponse<PaymentUrlResponse>.SuccessResponse(
            payment.ToUrlResponse(paymentUrl),
            "Tạo giao dịch thanh toán thành công");
    }

    public async Task<ApiResponse<bool>> HandleVNPayCallbackAsync(VNPayCallbackRequest request, string rawQueryString)
    {
        var isValid = vnPayService.ValidateCallback(rawQueryString, out var callbackResult);

        if (!isValid)
        {
            logger.LogWarning("VNPay signature FAILED — TxnRef: {TxnRef}", request.vnp_TxnRef);
            return ApiResponse<bool>.FailureResponse("Chữ ký xác thực không hợp lệ");
        }

        var payment = await unitOfWork.PaymentRepository.AsQueryable()
            .Include(p => p.Order)
                .ThenInclude(o => o!.OrderItems)
            .Include(p => p.InstructorWallet)
            .FirstOrDefaultAsync(p => p.PaymentNumber == callbackResult.TxnRef);

        if (payment == null)
        {
            logger.LogWarning("Payment not found — TxnRef: {TxnRef}", callbackResult.TxnRef);
            return ApiResponse<bool>.FailureResponse("Không tìm thấy giao dịch thanh toán");
        }

        // Idempotency: skip already-processed payments
        if (payment.Status is PaymentStatus.Completed or PaymentStatus.Failed)
        {
            logger.LogInformation("Payment already processed — PaymentId: {PaymentId}, Status: {Status}",
                payment.Id, payment.Status);
            return ApiResponse<bool>.SuccessResponse(true, "Giao dịch đã được xử lý trước đó");
        }

        if (callbackResult.Amount != payment.Amount)
        {
            logger.LogWarning("Amount mismatch — Expected: {Expected}, Received: {Received}",
                payment.Amount, callbackResult.Amount);
            return ApiResponse<bool>.FailureResponse("Số tiền thanh toán không khớp");
        }

        if (callbackResult.IsSuccess)
        {
            // Branch based on payment purpose
            if (payment.Purpose == PaymentPurpose.WalletTopUp)
                await HandleTopUpSuccessAsync(payment, callbackResult);
            else
                await HandlePaymentSuccessAsync(payment, callbackResult);
        }
        else
        {
            await HandlePaymentFailureAsync(payment, callbackResult);
        }

        return ApiResponse<bool>.SuccessResponse(true, "Xử lý callback thành công");
    }

    public async Task<ApiResponse<bool>> ConfirmPaymentAsync(string transactionId)
    {
        var payment = await unitOfWork.PaymentRepository.AsQueryable()
            .FirstOrDefaultAsync(p => p.ExternalTransactionId == transactionId);

        if (payment == null)
            return ApiResponse<bool>.FailureResponse("Không tìm thấy giao dịch thanh toán");

        return ApiResponse<bool>.SuccessResponse(
            payment.Status == PaymentStatus.Completed,
            payment.Status == PaymentStatus.Completed
                ? "Giao dịch đã được xác nhận"
                : $"Trạng thái giao dịch: {payment.Status}");
    }

    public async Task<ApiResponse<PaymentResponse>> CheckPaymentStatusAsync(Guid paymentId)
    {
        var payment = await unitOfWork.PaymentRepository.AsQueryable()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == paymentId);

        if (payment == null)
            return ApiResponse<PaymentResponse>.FailureResponse("Không tìm thấy giao dịch thanh toán");

        // Auto-expire pending payments past their deadline
        if (payment.Status == PaymentStatus.Pending && payment.ExpiredAt < DateTime.UtcNow)
        {
            var trackedPayment = await unitOfWork.PaymentRepository.AsQueryable()
                .FirstOrDefaultAsync(p => p.Id == paymentId);
            if (trackedPayment != null)
            {
                trackedPayment.Status = PaymentStatus.Expired;
                trackedPayment.UpdatedAt = DateTime.UtcNow;
                await unitOfWork.SaveChangesAsync();
                return ApiResponse<PaymentResponse>.SuccessResponse(
                    trackedPayment.ToResponse(), "Giao dịch đã hết hạn");
            }
        }

        return ApiResponse<PaymentResponse>.SuccessResponse(
            payment.ToResponse(), "Lấy trạng thái thanh toán thành công");
    }

    public async Task<ApiResponse<List<PaymentResponse>>> GetPaymentsByOrderAsync(Guid orderId)
    {
        var order = await unitOfWork.OrderRepository.FindOneAsync(o => o.Id == orderId);
        if (order == null)
            return ApiResponse<List<PaymentResponse>>.FailureResponse("Đơn hàng không tồn tại");

        var payments = await unitOfWork.PaymentRepository.AsQueryable()
            .Where(p => p.OrderId == orderId)
            .OrderByDescending(p => p.CreatedAt)
            .AsNoTracking()
            .ToListAsync();

        return ApiResponse<List<PaymentResponse>>.SuccessResponse(
            payments.Select(p => p.ToResponse()).ToList(),
            "Lấy danh sách thanh toán thành công");
    }

    public async Task<ApiResponse<List<PaymentResponse>>> GetPaymentsByUserAsync(
        PaginationRequest pagination, Guid userId)
    {
        var payments = await unitOfWork.PaymentRepository.GetPagedAsync(
            pageNumber: pagination.PageNumber,
            pageSize: pagination.PageSize,
            filter: p => p.Order!.UserId == userId,
            orderBy: q => q.OrderByDescending(p => p.CreatedAt));

        return ApiResponse<List<PaymentResponse>>.SuccessPagedResponse(
            payments.Items.Select(p => p.ToResponse()).ToList(),
            payments.TotalCount, pagination.PageNumber, pagination.PageSize,
            "Lấy lịch sử thanh toán thành công");
    }

    /// <summary>
    /// Instructor top-up wallet via VNPay.
    /// Creates a Payment with Purpose=WalletTopUp and generates VNPay URL.
    /// </summary>
    public async Task<ApiResponse<PaymentUrlResponse>> ProcessTopUpAsync(
        Guid instructorId, decimal amount, string returnUrl, string ipAddress)
    {
        // Verify wallet exists
        var wallet = await unitOfWork.InstructorWalletRepository.AsQueryable()
            .FirstOrDefaultAsync(w => w.InstructorId == instructorId);

        if (wallet == null)
            return ApiResponse<PaymentUrlResponse>.FailureResponse("Không tìm thấy ví giảng viên. Vui lòng liên hệ admin.");

        if (!wallet.IsActive)
            return ApiResponse<PaymentUrlResponse>.FailureResponse("Ví giảng viên đã bị vô hiệu hóa");

        var payment = new Payment
        {
            OrderId = null,
            Purpose = PaymentPurpose.WalletTopUp,
            WalletId = wallet.Id,
            PaymentNumber = GeneratePaymentNumber(),
            Status = PaymentStatus.Pending,
            Amount = amount,
            Currency = "VND",
            Provider = "VNPay",
            PaymentMethod = "VNPay",
            ExpiredAt = DateTime.UtcNow.AddMinutes(15),
            CreatedAt = DateTime.UtcNow
        };

        await unitOfWork.PaymentRepository.AddAsync(payment);
        await unitOfWork.SaveChangesAsync();

        // Generate VNPay URL for top-up
        var vietnamTz = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        var nowVietnam = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTz);
        var expireVietnam = payment.ExpiredAt.HasValue
            ? TimeZoneInfo.ConvertTimeFromUtc(payment.ExpiredAt.Value, vietnamTz)
            : nowVietnam.AddMinutes(15);

        var paymentInfo = new VNPayPaymentInfo
        {
            OrderId = wallet.Id.ToString(),
            PaymentNumber = payment.PaymentNumber,
            Amount = payment.Amount,
            OrderInfo = $"Nap tien vi giang vien {instructorId}",
            CreatedDate = nowVietnam,
            ExpireDate = expireVietnam,
            ReturnUrl = returnUrl
        };

        var paymentUrl = vnPayService.CreatePaymentUrl(paymentInfo, ipAddress);

        logger.LogInformation(
            "TopUp payment initiated — PaymentNumber: {PaymentNumber}, InstructorId: {InstructorId}, Amount: {Amount}",
            payment.PaymentNumber, instructorId, amount);

        return ApiResponse<PaymentUrlResponse>.SuccessResponse(
            payment.ToUrlResponse(paymentUrl),
            "Tạo giao dịch nạp tiền thành công");
    }

    // ── Private Helpers ──

    private async Task HandlePaymentSuccessAsync(Payment payment, VNPayCallbackResult result)
    {
        var order = payment.Order;

        payment.Status = PaymentStatus.Completed;
        payment.PaidAt = DateTime.UtcNow;
        payment.ExternalTransactionId = result.TransactionNo;
        payment.PaymentMethod = result.CardType;
        payment.UpdatedAt = DateTime.UtcNow;

        order.Status = OrderStatus.Paid;
        order.PaidAt = DateTime.UtcNow;
        order.UpdatedAt = DateTime.UtcNow;

        // ── Revenue Split & Wallet Credit (Per BR-19: 70% Instructor / 30% Platform) ──
        await CreditInstructorEarningsAsync(order);

        // ── Record coupon usage (Bug fix #1) ──
        if (order.SystemCouponId.HasValue)
        {
            await couponUsageService.RecordUsageAsync(new Dtos.CouponUsages.CreateCouponUsageRequest
            {
                CouponId = order.SystemCouponId.Value,
                UserId = order.UserId,
                OrderId = order.Id,
                DiscountApplied = order.DiscountAmount
            });
        }

        await unitOfWork.SaveChangesAsync();

        // ── Remove purchased items from cart (hard delete) ──
        await RemovePurchasedItemsFromCartAsync(order.UserId, order.OrderItems.Select(oi => oi.CourseId).ToList());

        // Notify Learning service to create Enrollment
        var courseIds = order.OrderItems.Select(oi => oi.CourseId).ToList();
        await publishEndpoint.Publish(new OrderCompletedEvent(order.Id, order.UserId, courseIds));

        // Invalidate excluded courses cache so GetAllCourses hides purchased courses immediately
        await publishEndpoint.Publish(new CacheInvalidateEvent($"excluded_courses:{order.UserId}"));

        logger.LogInformation(
            "Payment SUCCESS — PaymentId: {PaymentId}, OrderId: {OrderId}, Amount: {Amount}, TxnNo: {TxnNo}",
            payment.Id, order.Id, payment.Amount, result.TransactionNo);
    }

    /// <summary>
    /// Credit instructor wallets and platform wallet after payment success.
    /// Per BR-19: 70% Instructor / 30% Platform.
    /// Coupon discount attribution:
    ///   - Instructor coupon (ApplicableInstructorId != null) → Instructor absorbs discount (from held funds)
    ///   - System coupon (ApplicableInstructorId == null) → Platform absorbs discount (balance can go negative)
    /// </summary>
    private async Task CreditInstructorEarningsAsync(Order order)
    {
        // Load coupons to determine discount attribution
        Coupon? instructorCoupon = null;
        Coupon? systemCoupon = null;

        if (order.InstructorCouponId.HasValue)
        {
            instructorCoupon = await unitOfWork.CouponRepository.AsQueryable()
                .FirstOrDefaultAsync(c => c.Id == order.InstructorCouponId.Value);
        }

        if (order.SystemCouponId.HasValue)
        {
            systemCoupon = await unitOfWork.CouponRepository.AsQueryable()
                .FirstOrDefaultAsync(c => c.Id == order.SystemCouponId.Value);
        }

        // Group order items by instructor to aggregate earnings
        var instructorGroups = order.OrderItems.GroupBy(oi => oi.InstructorId);
        decimal totalPlatformFee = 0;

        foreach (var group in instructorGroups)
        {
            var instructorId = group.Key;
            decimal totalInstructorEarnings = 0;
            decimal groupPlatformFee = 0;

            foreach (var item in group)
            {
                // Recalculate revenue split with coupon discount attribution
                var earnings = CalculateItemRevenueSplit(item, order, instructorCoupon, systemCoupon);

                // Update OrderItem with final revenue split values
                item.InstructorEarnings = earnings.InstructorEarnings;
                item.PlatformFeeAmount = earnings.PlatformFee;

                totalInstructorEarnings += earnings.InstructorEarnings;
                groupPlatformFee += earnings.PlatformFee;
            }

            totalPlatformFee += groupPlatformFee;

            if (totalInstructorEarnings > 0)
            {
                logger.LogInformation("Crediting instructor {InstructorId} with earnings {Amount} for order {OrderId}",
                    instructorId, totalInstructorEarnings, order.Id);

                // Credit instructor wallet immediately (Phase 2 — no escrow)
                await walletService.CreditEarningsAsync(
                    instructorId,
                    totalInstructorEarnings,
                    order.Id,
                    $"Doanh thu đơn hàng #{order.OrderNumber}");
            }

            // ── Instructor coupon: deduct actual discount from held funds ──
            if (instructorCoupon != null && instructorCoupon.ApplicableInstructorId == instructorId && order.InstructorDiscountAmount > 0)
            {
                // Calculate this instructor group's share of the instructor discount
                var instructorItems = group.ToList();
                var instructorSubTotal = instructorItems.Sum(i => i.OriginalPrice);
                var discountRatio = order.SubTotal > 0 ? instructorSubTotal / order.SubTotal : 0;
                var instructorDiscount = order.InstructorDiscountAmount * discountRatio;

                if (instructorDiscount > 0)
                {
                    await walletService.DeductCouponUsageFromHoldAsync(
                        instructorId,
                        instructorDiscount,
                        instructorCoupon.Id,
                        order.Id,
                        $"Chi phí coupon {instructorCoupon.Code} cho đơn hàng #{order.OrderNumber}");

                    // Update coupon's remaining hold
                    instructorCoupon.RemainingHoldAmount = Math.Max(0, instructorCoupon.RemainingHoldAmount - instructorDiscount);

                    // If coupon is fully used, release any remaining hold
                    if (instructorCoupon.UsageLimit.HasValue && instructorCoupon.UsedCount >= instructorCoupon.UsageLimit.Value && instructorCoupon.RemainingHoldAmount > 0)
                    {
                        await walletService.ReleaseCouponHoldAsync(
                            instructorId,
                            instructorCoupon.RemainingHoldAmount,
                            instructorCoupon.Id,
                            $"Hoàn trả số dư giữ còn lại cho coupon {instructorCoupon.Code} (đã dùng hết)");

                        instructorCoupon.RemainingHoldAmount = 0;
                    }
                }
            }
        }

        // ── Credit Platform Wallet ──
        if (totalPlatformFee > 0)
        {
            await platformWalletService.CreditPlatformRevenueAsync(
                totalPlatformFee, order.Id,
                $"Hoa hồng 30% đơn hàng #{order.OrderNumber}");
        }

        // ── System coupon: Platform absorbs discount (balance can go negative) ──
        if (systemCoupon != null && order.SystemDiscountAmount > 0)
        {
            await platformWalletService.DebitSystemCouponCostAsync(
                order.SystemDiscountAmount, order.Id,
                $"Chi phí coupon hệ thống {systemCoupon.Code} cho đơn hàng #{order.OrderNumber}");
        }
    }

    /// <summary>
    /// Calculate revenue split for a single order item considering coupon discount attribution.
    /// Formula:
    ///   - Instructor coupon: Instructor absorbs instructor discount
    ///   - System coupon: Platform absorbs system discount
    ///   - Both coupons: Instructor absorbs instructor discount, platform absorbs system discount
    ///   - Final instructor earnings = (OriginalPrice - InstructorDiscount - SystemDiscount) * 0.7
    /// </summary>
    private static (decimal InstructorEarnings, decimal PlatformFee) CalculateItemRevenueSplit(
        OrderItem item, Order order, Coupon? instructorCoupon, Coupon? systemCoupon)
    {
        const decimal platformFeeRate = 0.30m; // Per BR-19: 30% platform
        var originalPrice = item.OriginalPrice;
        var finalPrice = originalPrice;

        // Calculate total original price of all items (before any discounts)
        var totalOriginalPrice = order.OrderItems.Sum(oi => oi.OriginalPrice);

        // Calculate instructor discount (absorbed by instructor)
        decimal instructorDiscount = 0;
        if (instructorCoupon != null && order.InstructorDiscountAmount > 0 && totalOriginalPrice > 0)
        {
            // Proportional discount for this item based on original price
            var discountRatio = originalPrice / totalOriginalPrice;
            instructorDiscount = order.InstructorDiscountAmount * discountRatio;
        }

        // Calculate system discount (absorbed by platform)
        decimal systemDiscount = 0;
        if (systemCoupon != null && order.SystemDiscountAmount > 0 && totalOriginalPrice > 0)
        {
            // Proportional discount for this item based on price after instructor discount
            var itemPriceAfterInstructorDiscount = originalPrice - instructorDiscount;
            var totalPriceAfterInstructorDiscount = totalOriginalPrice - order.InstructorDiscountAmount;
            if (totalPriceAfterInstructorDiscount > 0)
            {
                var discountRatio = itemPriceAfterInstructorDiscount / totalPriceAfterInstructorDiscount;
                systemDiscount = order.SystemDiscountAmount * discountRatio;
            }
        }

        // Final price after all discounts
        finalPrice = originalPrice - instructorDiscount - systemDiscount;

        // Instructor gets 70% of final price, platform gets 30%
        var instructorEarnings = finalPrice * (1 - platformFeeRate);
        var platformFee = finalPrice - instructorEarnings;

        return (instructorEarnings, platformFee);
    }

    /// <summary>
    /// Handle successful top-up payment. Credit instructor wallet.
    /// </summary>
    private async Task HandleTopUpSuccessAsync(Payment payment, VNPayCallbackResult result)
    {
        payment.Status = PaymentStatus.Completed;
        payment.PaidAt = DateTime.UtcNow;
        payment.ExternalTransactionId = result.TransactionNo;
        payment.PaymentMethod = result.CardType;
        payment.UpdatedAt = DateTime.UtcNow;

        await unitOfWork.SaveChangesAsync();

        // Credit instructor wallet
        if (payment.InstructorWallet != null)
        {
            await walletService.CreditTopUpAsync(
                payment.InstructorWallet.InstructorId,
                payment.Amount,
                payment.Id,
                $"Nạp tiền qua VNPay - Mã GD: {result.TransactionNo}");
        }

        logger.LogInformation(
            "TopUp SUCCESS — PaymentId: {PaymentId}, WalletId: {WalletId}, Amount: {Amount}, TxnNo: {TxnNo}",
            payment.Id, payment.WalletId, payment.Amount, result.TransactionNo);
    }

    private async Task HandlePaymentFailureAsync(Payment payment, VNPayCallbackResult result)
    {
        payment.Status = PaymentStatus.Failed;
        payment.FailureReason = result.ResponseDescription;
        payment.ExternalTransactionId = result.TransactionNo;
        payment.UpdatedAt = DateTime.UtcNow;

        await unitOfWork.SaveChangesAsync();

        // Bug fix #4: Do NOT remove cart items on payment failure.
        // Cart items should remain so user can retry payment.

        logger.LogWarning(
            "Payment FAILED — PaymentId: {PaymentId}, OrderId: {OrderId}, Code: {Code}, Reason: {Reason}",
            payment.Id, payment.OrderId, result.ResponseCode, result.ResponseDescription);
    }

    private string GenerateVNPayUrl(Payment payment, Order order, string ipAddress, string? returnUrl)
    {
        // VNPay requires timestamps in GMT+7
        var vietnamTz = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        var nowVietnam = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTz);
        var expireVietnam = payment.ExpiredAt.HasValue
            ? TimeZoneInfo.ConvertTimeFromUtc(payment.ExpiredAt.Value, vietnamTz)
            : nowVietnam.AddMinutes(15);

        var paymentInfo = new VNPayPaymentInfo
        {
            OrderId = order.Id.ToString(),
            PaymentNumber = payment.PaymentNumber,
            Amount = payment.Amount,
            OrderInfo = $"Thanh toan don hang {order.OrderNumber}",
            CreatedDate = nowVietnam,
            ExpireDate = expireVietnam,
            ReturnUrl = returnUrl
        };

        return vnPayService.CreatePaymentUrl(paymentInfo, ipAddress);
    }

    private static string GeneratePaymentNumber()
        => $"PAY-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";

    /// <summary>
    /// Remove purchased items from user's cart after successful payment.
    /// </summary>
    private async Task RemovePurchasedItemsFromCartAsync(Guid userId, List<Guid> purchasedCourseIds)
    {
        try
        {
            // Find cart items for purchased courses
            var cartItems = await unitOfWork.CartItemRepository.AsQueryable()
                .Where(ci => ci.Cart.UserId == userId && purchasedCourseIds.Contains(ci.CourseId))
                .ToListAsync();

            if (!cartItems.Any())
            {
                logger.LogInformation("No cart items to remove for user {UserId}", userId);
                return;
            }

            // Hard delete cart items
            foreach (var item in cartItems)
            {
                await unitOfWork.CartItemRepository.DeleteAsync(item.Id);
            }

            await unitOfWork.SaveChangesAsync();

            logger.LogInformation(
                "Removed {Count} purchased items from cart for user {UserId}",
                cartItems.Count, userId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to remove purchased items from cart for user {UserId}", userId);
            // Don't throw - payment already succeeded, cart cleanup is non-critical
        }
    }
}
