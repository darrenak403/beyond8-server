using Beyond8.Integration.Application.Clients;
using Beyond8.Integration.Application.Dtos.Clients.Identity;

namespace Beyond8.Integration.Application.Helpers
{
    public static class SubscriptionHelper
    {
        public static async Task<SubscriptionCheckResult> CheckSubscriptionStatusAsync(
            IIdentityClient identityClient,
            Guid userId)
        {
            try
            {
                var response = await identityClient.GetUserSubscriptionAsync(userId);

                if (!response.IsSuccess || response.Data == null)
                {
                    return new SubscriptionCheckResult
                    {
                        IsAllowed = false,
                        Message = "Không thể lấy thông tin gói đăng ký hoặc dữ liệu trống."
                    };
                }

                var data = response.Data;
                var planName = data.SubscriptionPlan?.Name;

                if (!data.IsRequestLimitedReached)
                {
                    return new SubscriptionCheckResult
                    {
                        IsAllowed = true,
                        Message = "Gói đăng ký hợp lệ."
                    };
                }

                var isFreePlan = string.Equals(planName, "FREE", StringComparison.OrdinalIgnoreCase);

                if (isFreePlan)
                {
                    return new SubscriptionCheckResult
                    {
                        IsAllowed = false,
                        Message = "Bạn đã hết số lần sử dụng AI miễn phí. Vui lòng đăng ký gói dịch vụ để tiếp tục."
                    };
                }

                return new SubscriptionCheckResult
                {
                    IsAllowed = false,
                    Message = $"Bạn đã hết số lần sử dụng AI với gói {planName}. Vui lòng gia hạn hoặc mua thêm.",
                    RequestLimitedEndsAt = data.RequestLimitedEndsAt,
                    Metadata = data.RequestLimitedEndsAt != null ? new { RecoverTime = data.RequestLimitedEndsAt } : null
                };
            }
            catch (Exception ex)
            {
                return new SubscriptionCheckResult
                {
                    IsAllowed = false,
                    Message = "Lỗi hệ thống khi kiểm tra quyền truy cập.",
                    Metadata = new { ErrorDetail = ex.Message }
                };
            }
        }

        public const int UsageQuotaPerRequest = 1;
        public static async Task UpdateUsageQuotaAsync(
            IIdentityClient identityClient,
            Guid userId,
            int numberOfRequests = UsageQuotaPerRequest)
        {
            await identityClient.UpdateUserSubscriptionAsync(userId, new UpdateUsageQuotaRequest
            {
                NumberOfRequests = numberOfRequests
            });
        }
    }
}