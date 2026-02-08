using Beyond8.Common;
using Beyond8.Common.Utilities;
using Beyond8.Sale.Application.Dtos.Payouts;

namespace Beyond8.Sale.Application.Services.Interfaces;

public interface IPayoutService
{
    // ── Instructor ──
    Task<ApiResponse<PayoutRequestResponse>> CreatePayoutRequestAsync(CreatePayoutRequest request);
    Task<ApiResponse<PayoutRequestResponse>> GetPayoutRequestByIdAsync(Guid payoutId);
    Task<ApiResponse<List<PayoutRequestResponse>>> GetPayoutRequestsByInstructorAsync(Guid instructorId, PaginationRequest pagination);

    // ── Admin Only ──
    Task<ApiResponse<bool>> ApprovePayoutRequestAsync(Guid payoutId);
    Task<ApiResponse<bool>> RejectPayoutRequestAsync(Guid payoutId, string reason);
    Task<ApiResponse<List<PayoutRequestResponse>>> GetPayoutRequestsAsync(PaginationRequest pagination);
}