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
using System;
using System.Linq;
using Beyond8.Sale.Application.Clients.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Beyond8.Sale.Application.Dtos.Orders;
using Beyond8.Sale.Application.Mappings.Orders;
using System.Text.Json;

namespace Beyond8.Sale.Application.Services.Implements;

public class PaymentService(
    ILogger<PaymentService> logger,
    IUnitOfWork unitOfWork,
    IVNPayService vnPayService,
    IInstructorWalletService walletService,
    IPlatformWalletService platformWalletService,
    ICouponUsageService couponUsageService,
    IPublishEndpoint publishEndpoint,
    IIdentityClient identityClient) : IPaymentService
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
            else if (payment.Purpose == PaymentPurpose.Subscription)
                await HandleSubscriptionSuccessAsync(payment, callbackResult);
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
                trackedPayment.PaymentUrl = null; // Clear stored payment URL on expiry
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
        var candidates = await unitOfWork.PaymentRepository.AsQueryable()
            .Include(p => p.Order!)
                .ThenInclude(o => o!.OrderItems)
            .Include(p => p.InstructorWallet)
            .Where(p => (p.Order != null && p.Order.UserId == userId)
                        || p.TargetUserId == userId
                        || (p.Purpose == PaymentPurpose.WalletTopUp && p.InstructorWallet != null && p.InstructorWallet.InstructorId == userId)
                        || (p.Purpose == PaymentPurpose.Subscription && p.TargetUserId == null && p.Metadata != null))
            .OrderByDescending(p => p.CreatedAt)
            .AsNoTracking()
            .ToListAsync();

        var paymentsForUser = new List<Payment>();
        foreach (var p in candidates)
        {
            if ((p.Order != null && p.Order.UserId == userId) 
                || p.TargetUserId == userId
                || (p.Purpose == PaymentPurpose.WalletTopUp && p.InstructorWallet != null && p.InstructorWallet.InstructorId == userId))
            {
                paymentsForUser.Add(p);
                continue;
            }

            if (p.Purpose == PaymentPurpose.Subscription && !string.IsNullOrEmpty(p.Metadata))
            {
                try
                {
                    var md = JsonSerializer.Deserialize<SubscriptionPaymentMetadata>(p.Metadata, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (md?.UserId == userId)
                    {
                        paymentsForUser.Add(p);
                    }
                }
                catch { }
            }
        }

        var total = paymentsForUser.Count;
        var items = paymentsForUser
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(p => p.ToResponse())
            .ToList();

        return ApiResponse<List<PaymentResponse>>.SuccessPagedResponse(
            items,
            total, pagination.PageNumber, pagination.PageSize,
            "Lấy lịch sử thanh toán thành công");
    }

    // Run cleanup of expired payments (migrated from PaymentCleanupService)
    public async Task RunCleanupAsync()
    {
        var now = DateTime.UtcNow;
        var expiredPayments = await unitOfWork.PaymentRepository.AsQueryable()
            .Where(p => p.Status == PaymentStatus.Pending && p.ExpiredAt < now)
            .ToListAsync();

        if (!expiredPayments.Any())
        {
            logger.LogDebug("No expired payments found to clean up at {Now}.", now);
            return;
        }

        logger.LogInformation("Found {Count} expired payments to clean up.", expiredPayments.Count);

        foreach (var payment in expiredPayments)
        {
            try
            {
                payment.Status = PaymentStatus.Expired;
                payment.PaymentUrl = null;
                payment.FailureReason = "Payment expired after 15 minutes";
                payment.UpdatedAt = DateTime.UtcNow;

                if (payment.OrderId.HasValue)
                {
                    var order = await unitOfWork.OrderRepository.FindOneAsync(o => o.Id == payment.OrderId.Value);
                    if (order != null && order.Status == Domain.Enums.OrderStatus.Pending)
                    {
                        logger.LogInformation("Order {OrderId} remains pending for retry after payment {PaymentId} expired.", order.Id, payment.Id);
                    }
                }

                logger.LogInformation("Marked payment {PaymentId} as expired and cleared URL.", payment.Id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to mark payment {PaymentId} as expired.", payment.Id);
            }
        }

        await unitOfWork.SaveChangesAsync();
        logger.LogInformation("Successfully cleaned up {Count} expired payments.", expiredPayments.Count);
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

        var pendingCheck = await CheckPendingPaymentAsync(PaymentPurpose.WalletTopUp, instructorId);
        if (pendingCheck.IsSuccess && pendingCheck.Data != null && pendingCheck.Data.PaymentInfo != null)
        {
            var existingInfo = pendingCheck.Data.PaymentInfo;
            existingInfo.IsPending = true;
            return ApiResponse<PaymentUrlResponse>.SuccessResponse(
                existingInfo,
                "Đã có giao dịch nạp tiền đang chờ xử lý. Vui lòng hoàn tất giao dịch trước khi tạo giao dịch mới.");
        }

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

        payment.PaymentUrl = paymentUrl;
        await unitOfWork.SaveChangesAsync();
        logger.LogInformation(
            "TopUp payment initiated — PaymentNumber: {PaymentNumber}, InstructorId: {InstructorId}, Amount: {Amount}",
            payment.PaymentNumber, instructorId, amount);

        return ApiResponse<PaymentUrlResponse>.SuccessResponse(
            payment.ToUrlResponse(paymentUrl),
            "Tạo giao dịch nạp tiền thành công");
    }


    public async Task<ApiResponse<PendingPaymentResponse>> CheckPendingPaymentAsync(PaymentPurpose purpose, Guid userId)
    {
        try
        {
            var matched = await unitOfWork.PaymentRepository.AsQueryable()
                .Include(p => p.Order)
                .Include(p => p.InstructorWallet)
                .Where(p => p.Purpose == purpose
                            && (p.Status == PaymentStatus.Pending || p.Status == PaymentStatus.Processing)
                            && p.ExpiredAt > DateTime.UtcNow
                            && ((p.Order != null && p.Order.UserId == userId)
                                || p.TargetUserId == userId
                                || (p.InstructorWallet != null && p.InstructorWallet.InstructorId == userId)))
                .OrderByDescending(p => p.CreatedAt)
                .FirstOrDefaultAsync();

            if (matched != null)
            {
                // Return PendingPaymentResponse using mapping helper
                var pending = matched.ToPendingPaymentResponse();
                return ApiResponse<PendingPaymentResponse>.SuccessResponse(pending, "Đã có giao dịch đang chờ xử lý");
            }

            return ApiResponse<PendingPaymentResponse>.SuccessResponse(null!, "Không có giao dịch đang chờ xử lý");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking pending payments for Purpose: {Purpose}, UserId: {UserId}", purpose, userId);
            return ApiResponse<PendingPaymentResponse>.FailureResponse("Lỗi khi kiểm tra giao dịch đang chờ xử lý");
        }
    }

    public async Task<ApiResponse<PaymentUrlResponse>> ProcessSubscriptionAsync(string planCode, Guid userId, string returnUrl, string ipAddress)
    {
        if (string.IsNullOrWhiteSpace(planCode))
            return ApiResponse<PaymentUrlResponse>.FailureResponse("PlanCode không được để trống");


        var pendingCheck = await CheckPendingPaymentAsync(PaymentPurpose.Subscription, userId);
        if (pendingCheck.IsSuccess && pendingCheck.Data != null && pendingCheck.Data.PaymentInfo != null)
        {
            var existingInfo = pendingCheck.Data.PaymentInfo;
            existingInfo.IsPending = true;
            return ApiResponse<PaymentUrlResponse>.SuccessResponse(
                existingInfo,
                "Đã có giao dịch mua subscription đang chờ xử lý. Vui lòng hoàn tất giao dịch trước khi mua gói mới.");
        }

        // Get plans from Identity service and find the requested plan by code
        var plansResponse = await identityClient.GetSubscriptionPlansAsync();
        if (!plansResponse.IsSuccess)
        {
            logger.LogWarning("Failed to fetch subscription plans from Identity: {Message}", plansResponse.Message);
            return ApiResponse<PaymentUrlResponse>.FailureResponse("Không thể lấy thông tin gói đăng ký");
        }

        var plan = plansResponse.Data?.FirstOrDefault(p => string.Equals(p.Code, planCode, StringComparison.OrdinalIgnoreCase));
        if (plan == null)
            return ApiResponse<PaymentUrlResponse>.FailureResponse("Gói đăng ký không tồn tại hoặc đã bị vô hiệu hóa");

        // Create payment with correct amount from plan.Price
        var payment = new Payment
        {
            OrderId = null,
            Purpose = PaymentPurpose.Subscription,
            // Record explicit target user for reliable DB-side pending checks
            TargetUserId = userId,
            PaymentNumber = GeneratePaymentNumber(),
            Status = PaymentStatus.Pending,
            Amount = plan.Price,
            Currency = "VND",
            Provider = "VNPay",
            PaymentMethod = "VNPay",
            ExpiredAt = DateTime.UtcNow.AddMinutes(15),
            CreatedAt = DateTime.UtcNow
        };

        var details = new
        {
            PlanId = plan.Id,
            PlanCode = plan.Code,
            UserId = userId,
            // Indicate this payment is intended to override any existing subscription
            OverrideExisting = true
        };

        payment.Metadata = System.Text.Json.JsonSerializer.Serialize(details);

        await unitOfWork.PaymentRepository.AddAsync(payment);
        await unitOfWork.SaveChangesAsync();

        var vietnamTz = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        var nowVietnam = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTz);
        var expireVietnam = payment.ExpiredAt.HasValue
            ? TimeZoneInfo.ConvertTimeFromUtc(payment.ExpiredAt.Value, vietnamTz)
            : nowVietnam.AddMinutes(15);

        var paymentInfo = new VNPayPaymentInfo
        {
            OrderId = payment.Id.ToString(),
            PaymentNumber = payment.PaymentNumber,
            Amount = payment.Amount,
            // Use ASCII-friendly order info to avoid encoding issues on provider side
            OrderInfo = $"Buy subscription {plan.Code}",
            CreatedDate = nowVietnam,
            ExpireDate = expireVietnam,
            ReturnUrl = returnUrl
        };

        var paymentUrl = vnPayService.CreatePaymentUrl(paymentInfo, ipAddress);

        // Save payment URL
        payment.PaymentUrl = paymentUrl;
        await unitOfWork.SaveChangesAsync();

        logger.LogInformation("Subscription payment initiated — PaymentNumber: {PaymentNumber}, PlanCode: {PlanCode}, UserId: {UserId}",
            payment.PaymentNumber, plan.Code, userId);

        return ApiResponse<PaymentUrlResponse>.SuccessResponse(payment.ToUrlResponse(paymentUrl), "Tạo giao dịch mua subscription thành công");
    }

    // ── Private Helpers ──

    private async Task HandlePaymentSuccessAsync(Payment payment, VNPayCallbackResult result)
    {
        var order = payment.Order;
        if (order == null)
        {
            logger.LogWarning("Payment success but order not found: {PaymentId}", payment.Id);
            return;
        }

        payment.Status = PaymentStatus.Completed;
        payment.PaidAt = DateTime.UtcNow;
        payment.ExternalTransactionId = result.TransactionNo;
        payment.PaymentMethod = result.CardType;
        payment.UpdatedAt = DateTime.UtcNow;

        // Clear stored payment URL after successful processing
        payment.PaymentUrl = null;

        order.Status = OrderStatus.Paid;
        order.PaidAt = DateTime.UtcNow;
        order.UpdatedAt = DateTime.UtcNow;
        // Mark settlement eligibility date (PaidAt + 14 days)
        try
        {
            // Per BR-19: settlement eligibility at PaidAt + 14 days
            order.SettlementEligibleAt = order.PaidAt?.AddDays(14);
            order.IsSettled = false; // will be settled by background job
        }
        catch
        {
        }

        // ── Revenue Split & Wallet Credit (Per BR-19: 70% Instructor / 30% Platform) ──
        await CreditInstructorEarningsAsync(order);

        // ── Record coupon usage ──
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

                // Credit instructor wallet into pending/escrow (Phase 3 behavior). AvailableAt = PaidAt + 14 days
                var availableAt = order.SettlementEligibleAt ?? (order.PaidAt.HasValue ? order.PaidAt.Value.AddDays(14) : DateTime.UtcNow.AddDays(14));
                await walletService.CreditPendingAsync(
                    instructorId,
                    totalInstructorEarnings,
                    order.Id,
                    $"Doanh thu đơn hàng #{order.OrderNumber}",
                    availableAt);
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
        // Platform fee already has system coupon discount absorbed (from CalculateItemRevenueSplit)
        // No need for separate DebitSystemCouponCostAsync — would cause double-deduction
        if (totalPlatformFee != 0)
        {
            var availableAt = order.SettlementEligibleAt ?? (order.PaidAt.HasValue ? order.PaidAt.Value.AddDays(14) : DateTime.UtcNow.AddDays(14));

            if (totalPlatformFee > 0)
            {
                await platformWalletService.CreditPlatformRevenuePendingAsync(
                    totalPlatformFee, order.Id,
                    $"Hoa hồng 30% đơn hàng #{order.OrderNumber}" +
                    (systemCoupon != null ? $" (đã trừ coupon hệ thống {systemCoupon.Code})" : ""),
                    availableAt);
            }
            else
            {
                // When system discount exceeds 30% platform share, platform owes money
                await platformWalletService.DebitSystemCouponCostAsync(
                    Math.Abs(totalPlatformFee), order.Id,
                    $"Chi phí coupon hệ thống {systemCoupon?.Code} vượt hoa hồng đơn hàng #{order.OrderNumber}");
            }
        }
    }

    /// <summary>
    /// Calculate revenue split for a single order item considering coupon discount attribution.
    /// Formula (BR-19):
    ///   - Always split 70/30 from ORIGINAL PRICE
    ///   - Instructor coupon: Instructor absorbs discount (deducted from their 70%)
    ///   - System coupon: Platform absorbs discount (deducted from their 30%)
    /// </summary>
    private static (decimal InstructorEarnings, decimal PlatformFee) CalculateItemRevenueSplit(
        OrderItem item, Order order, Coupon? instructorCoupon, Coupon? systemCoupon)
    {
        const decimal instructorRate = 0.70m; // Per BR-19: 70% instructor
        const decimal platformFeeRate = 0.30m; // Per BR-19: 30% platform
        var originalPrice = item.OriginalPrice;

        // Calculate total original price of all items (before any discounts)
        var totalOriginalPrice = order.OrderItems.Sum(oi => oi.OriginalPrice);

        // Always calculate base split from ORIGINAL PRICE
        var baseInstructorEarnings = originalPrice * instructorRate;
        var basePlatformFee = originalPrice * platformFeeRate;

        // Calculate instructor discount (absorbed by instructor — deducted from their 70%)
        decimal instructorDiscount = 0;
        if (instructorCoupon != null && order.InstructorDiscountAmount > 0 && totalOriginalPrice > 0)
        {
            var discountRatio = originalPrice / totalOriginalPrice;
            instructorDiscount = order.InstructorDiscountAmount * discountRatio;
        }

        // Calculate system discount (absorbed by platform — deducted from their 30%)
        decimal systemDiscount = 0;
        if (systemCoupon != null && order.SystemDiscountAmount > 0 && totalOriginalPrice > 0)
        {
            var itemPriceAfterInstructorDiscount = originalPrice - instructorDiscount;
            var totalPriceAfterInstructorDiscount = totalOriginalPrice - order.InstructorDiscountAmount;
            if (totalPriceAfterInstructorDiscount > 0)
            {
                var discountRatio = itemPriceAfterInstructorDiscount / totalPriceAfterInstructorDiscount;
                systemDiscount = order.SystemDiscountAmount * discountRatio;
            }
        }

        // Each party absorbs their own coupon cost
        var instructorEarnings = baseInstructorEarnings - instructorDiscount;
        var platformFee = basePlatformFee - systemDiscount;

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

        // Clear stored payment URL after successful processing
        payment.PaymentUrl = null;

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

    private async Task HandleSubscriptionSuccessAsync(Payment payment, VNPayCallbackResult result)
    {
        payment.Status = PaymentStatus.Completed;
        payment.PaidAt = DateTime.UtcNow;
        payment.ExternalTransactionId = result.TransactionNo;
        payment.PaymentMethod = result.CardType;
        payment.UpdatedAt = DateTime.UtcNow;

        // Clear stored payment URL after successful processing
        payment.PaymentUrl = null;

        await unitOfWork.SaveChangesAsync();

        // Parse payment details to extract PlanId and UserId
        try
        {
            if (!string.IsNullOrEmpty(payment.Metadata))
            {
                var md = JsonSerializer.Deserialize<SubscriptionPaymentMetadata>(payment.Metadata, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                Guid planId = md?.PlanId ?? Guid.Empty;
                Guid userId = md?.UserId ?? payment.TargetUserId ?? Guid.Empty;

                // Fall back to PlanCode (older behavior) and resolve via Identity client
                if (planId == Guid.Empty && !string.IsNullOrEmpty(md?.PlanCode))
                {
                    try
                    {
                        var plansResponse = await identityClient.GetSubscriptionPlansAsync();
                        if (plansResponse.IsSuccess && plansResponse.Data != null)
                        {
                            var matched = plansResponse.Data.FirstOrDefault(p => string.Equals(p.Code, md.PlanCode, StringComparison.OrdinalIgnoreCase));
                            if (matched != null)
                                planId = matched.Id;
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "Failed to resolve PlanCode {PlanCode} to PlanId via Identity client", md.PlanCode);
                    }
                }

                if (planId == Guid.Empty)
                {
                    logger.LogWarning("Subscription payment succeeded but PlanId could not be resolved (PaymentId={PaymentId})", payment.Id);
                }
                else if (userId == Guid.Empty)
                {
                    logger.LogWarning("Subscription payment succeeded but UserId missing in payment metadata (PaymentId={PaymentId})", payment.Id);
                }
                else
                {
                    // Try to resolve plan duration to compute ExpiresAt
                    DateTime startedAt = DateTime.UtcNow;
                    DateTime? expiresAt = null;

                    try
                    {
                        var plansResponse2 = await identityClient.GetSubscriptionPlansAsync();
                        if (plansResponse2.IsSuccess && plansResponse2.Data != null)
                        {
                            var matchedPlan = plansResponse2.Data.FirstOrDefault(p => p.Id == planId);
                            if (matchedPlan != null && matchedPlan.DurationDays > 0)
                            {
                                expiresAt = startedAt.AddDays(matchedPlan.DurationDays);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "Failed to fetch subscription plan details to compute ExpiresAt for PlanId={PlanId}", planId);
                    }

                    // Publish SubscriptionPurchasedEvent for Identity to consume and activate subscription
                    await publishEndpoint.Publish(new SubscriptionPurchasedEvent(
                        payment.Id, // use payment.Id as OrderId placeholder
                        userId,
                        planId,
                        payment.Id,
                        startedAt,
                        expiresAt));

                    logger.LogInformation("Published SubscriptionPurchasedEvent for User {UserId}, Plan {PlanId}, ExpiresAt={ExpiresAt}", userId, planId, expiresAt);

                    // Credit platform wallet for subscription revenue. Subscription revenue is platform-owned.
                    try
                    {
                        // Subscription revenue should become available quickly (5 minutes), not the 14-day order settlement.
                        var subscriptionAvailableAt = DateTime.UtcNow.AddMinutes(5);
                        await platformWalletService.CreditPlatformRevenuePendingAsync(
                            payment.Amount,
                            payment.Id,
                            $"Subscription purchase {planId} for user {userId}",
                            subscriptionAvailableAt);
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "Failed to credit platform wallet for subscription payment {PaymentId}", payment.Id);
                    }
                }
            }
            else
            {
                logger.LogWarning("Subscription payment succeeded but PaymentDetails missing (PaymentId={PaymentId})", payment.Id);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to publish SubscriptionPurchasedEvent for PaymentId={PaymentId}", payment.Id);
        }

        logger.LogInformation(
            "Subscription Payment SUCCESS — PaymentId: {PaymentId}, Amount: {Amount}, TxnNo: {TxnNo}",
            payment.Id, payment.Amount, result.TransactionNo);
    }

    private async Task HandlePaymentFailureAsync(Payment payment, VNPayCallbackResult result)
    {
        payment.Status = PaymentStatus.Failed;
        payment.FailureReason = result.ResponseDescription;
        payment.ExternalTransactionId = result.TransactionNo;
        payment.UpdatedAt = DateTime.UtcNow;

        await unitOfWork.SaveChangesAsync();

        // ── Revert coupon usage if recorded (for backward compatibility) ──
        if (payment.OrderId.HasValue && payment.Order?.SystemCouponId.HasValue == true)
        {
            try
            {
                var existingUsage = await unitOfWork.CouponUsageRepository.AsQueryable()
                    .Include(u => u.Coupon)
                    .FirstOrDefaultAsync(u => u.OrderId == payment.OrderId);

                if (existingUsage != null)
                {
                    // Remove the usage record
                    await unitOfWork.CouponUsageRepository.DeleteAsync(existingUsage.Id);

                    // Decrement coupon usage count
                    if (existingUsage.Coupon != null)
                    {
                        existingUsage.Coupon.UsedCount = Math.Max(0, existingUsage.Coupon.UsedCount - 1);
                        existingUsage.Coupon.UpdatedAt = DateTime.UtcNow;
                    }

                    await unitOfWork.SaveChangesAsync();

                    logger.LogInformation("Reverted coupon usage for failed payment: {PaymentId}, OrderId: {OrderId}",
                        payment.Id, payment.OrderId);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to revert coupon usage for failed payment: {PaymentId}", payment.Id);
                // Don't throw - payment failure handling should not fail
            }
        }

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
