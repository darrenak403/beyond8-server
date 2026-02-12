using Beyond8.Common;
using Beyond8.Common.Utilities;
using Beyond8.Sale.Application.Dtos.Payouts;
using Beyond8.Sale.Application.Mappings.Payouts;
using Beyond8.Sale.Application.Services.Interfaces;
using Beyond8.Sale.Domain.Enums;
using Beyond8.Sale.Domain.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Beyond8.Sale.Application.Services.Implements;

public class PayoutService(
    ILogger<PayoutService> logger,
    IUnitOfWork unitOfWork,
    IInstructorWalletService walletService) : IPayoutService
{
    /// <summary>
    /// Instructor requests payout (min 500k VND per BR-19)
    /// Status: Requested → awaiting Admin approval
    /// </summary>
    public async Task<ApiResponse<PayoutRequestResponse>> CreatePayoutRequestAsync(CreatePayoutRequest request)
    {
        // Verify wallet exists and has sufficient balance
        var walletResult = await walletService.GetWalletByInstructorAsync(request.InstructorId);
        if (!walletResult.IsSuccess || walletResult.Data == null)
            return ApiResponse<PayoutRequestResponse>.FailureResponse("Không tìm thấy ví giảng viên");

        var wallet = walletResult.Data;

        if (wallet.AvailableBalance < request.Amount)
            return ApiResponse<PayoutRequestResponse>.FailureResponse(
                $"Số dư không đủ. Số dư hiện tại: {wallet.AvailableBalance:N0} VND");

        // Check for existing pending payout requests
        var existingPending = await unitOfWork.PayoutRequestRepository.AsQueryable()
            .AnyAsync(p => p.InstructorId == request.InstructorId
                           && p.Status == PayoutStatus.Requested
                           && p.DeletedAt == null);

        if (existingPending)
            return ApiResponse<PayoutRequestResponse>.FailureResponse(
                "Bạn đã có yêu cầu rút tiền đang chờ xử lý. Vui lòng đợi Admin phê duyệt.");

        var payout = request.ToEntity(wallet.Id);

        await unitOfWork.PayoutRequestRepository.AddAsync(payout);
        await unitOfWork.SaveChangesAsync();

        logger.LogInformation(
            "Payout request created — RequestNumber: {RequestNumber}, InstructorId: {InstructorId}, Amount: {Amount}",
            payout.RequestNumber, request.InstructorId, request.Amount);

        return ApiResponse<PayoutRequestResponse>.SuccessResponse(
            payout.ToResponse(), "Yêu cầu rút tiền đã được tạo thành công");
    }

    public async Task<ApiResponse<PayoutRequestResponse>> GetPayoutRequestByIdAsync(Guid payoutId)
    {
        var payout = await unitOfWork.PayoutRequestRepository.AsQueryable()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == payoutId && p.DeletedAt == null);

        if (payout == null)
            return ApiResponse<PayoutRequestResponse>.FailureResponse("Không tìm thấy yêu cầu rút tiền");

        return ApiResponse<PayoutRequestResponse>.SuccessResponse(
            payout.ToResponse(), "Lấy thông tin yêu cầu rút tiền thành công");
    }

    /// <summary>
    /// Admin approves payout → Deduct from wallet → Status: Approved → Processing → Completed
    /// Per REQ-07.09: Admin approval required
    /// </summary>
    public async Task<ApiResponse<bool>> ApprovePayoutRequestAsync(Guid payoutId, Guid adminUserId)
    {
        var payout = await unitOfWork.PayoutRequestRepository.AsQueryable()
            .FirstOrDefaultAsync(p => p.Id == payoutId && p.DeletedAt == null);

        if (payout == null)
            return ApiResponse<bool>.FailureResponse("Không tìm thấy yêu cầu rút tiền");

        if (payout.Status != PayoutStatus.Requested)
            return ApiResponse<bool>.FailureResponse(
                $"Chỉ có thể phê duyệt yêu cầu đang chờ xử lý. Trạng thái hiện tại: {payout.Status}");

        // Deduct from wallet (validates balance)
        var deductResult = await walletService.DeductForPayoutAsync(
            payout.InstructorId, payout.Amount, payout.Id,
            $"Rút tiền #{payout.RequestNumber}");

        if (!deductResult.IsSuccess)
            return ApiResponse<bool>.FailureResponse(deductResult.Message ?? "Không thể trừ tiền từ ví");

        // Update payout status — Phase 2: skip Processing, go directly to Completed (mock bank transfer)
        payout.Status = PayoutStatus.Completed;
        payout.ApprovedBy = adminUserId;
        payout.ApprovedAt = DateTime.UtcNow;
        payout.ProcessedAt = DateTime.UtcNow;
        payout.UpdatedAt = DateTime.UtcNow;

        await unitOfWork.SaveChangesAsync();

        logger.LogInformation(
            "Payout approved and completed — PayoutId: {PayoutId}, Amount: {Amount}, InstructorId: {InstructorId}",
            payoutId, payout.Amount, payout.InstructorId);

        return ApiResponse<bool>.SuccessResponse(true, "Yêu cầu rút tiền đã được phê duyệt và xử lý thành công");
    }

    /// <summary>
    /// Admin rejects payout → No balance change (funds not deducted yet per Phase 2 flow)
    /// </summary>
    public async Task<ApiResponse<bool>> RejectPayoutRequestAsync(Guid payoutId, string reason, Guid adminUserId)
    {
        var payout = await unitOfWork.PayoutRequestRepository.AsQueryable()
            .FirstOrDefaultAsync(p => p.Id == payoutId && p.DeletedAt == null);

        if (payout == null)
            return ApiResponse<bool>.FailureResponse("Không tìm thấy yêu cầu rút tiền");

        if (payout.Status != PayoutStatus.Requested)
            return ApiResponse<bool>.FailureResponse(
                $"Chỉ có thể từ chối yêu cầu đang chờ xử lý. Trạng thái hiện tại: {payout.Status}");

        payout.Status = PayoutStatus.Rejected;
        payout.RejectedBy = adminUserId;
        payout.RejectedAt = DateTime.UtcNow;
        payout.RejectionReason = reason;
        payout.UpdatedAt = DateTime.UtcNow;

        await unitOfWork.SaveChangesAsync();

        logger.LogInformation(
            "Payout rejected — PayoutId: {PayoutId}, Reason: {Reason}",
            payoutId, reason);

        return ApiResponse<bool>.SuccessResponse(true, "Đã từ chối yêu cầu rút tiền");
    }

    /// <summary>
    /// Admin gets all payout requests (paginated)
    /// </summary>
    public async Task<ApiResponse<List<PayoutRequestResponse>>> GetPayoutRequestsAsync(PaginationRequest pagination)
    {
        var payouts = await unitOfWork.PayoutRequestRepository.GetPagedAsync(
            pageNumber: pagination.PageNumber,
            pageSize: pagination.PageSize,
            filter: p => p.DeletedAt == null,
            orderBy: q => q.OrderByDescending(p => p.RequestedAt));

        return ApiResponse<List<PayoutRequestResponse>>.SuccessPagedResponse(
            payouts.Items.Select(p => p.ToResponse()).ToList(),
            payouts.TotalCount,
            pagination.PageNumber,
            pagination.PageSize,
            "Lấy danh sách yêu cầu rút tiền thành công");
    }

    /// <summary>
    /// Instructor gets their own payout requests (paginated)
    /// </summary>
    public async Task<ApiResponse<List<PayoutRequestResponse>>> GetPayoutRequestsByInstructorAsync(
        Guid instructorId, PaginationRequest pagination)
    {
        var payouts = await unitOfWork.PayoutRequestRepository.GetPagedAsync(
            pageNumber: pagination.PageNumber,
            pageSize: pagination.PageSize,
            filter: p => p.InstructorId == instructorId && p.DeletedAt == null,
            orderBy: q => q.OrderByDescending(p => p.RequestedAt));

        return ApiResponse<List<PayoutRequestResponse>>.SuccessPagedResponse(
            payouts.Items.Select(p => p.ToResponse()).ToList(),
            payouts.TotalCount,
            pagination.PageNumber,
            pagination.PageSize,
            "Lấy danh sách yêu cầu rút tiền thành công");
    }
}
