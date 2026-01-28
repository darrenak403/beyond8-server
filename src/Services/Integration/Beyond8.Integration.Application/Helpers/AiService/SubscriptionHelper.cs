using Beyond8.Integration.Application.Clients;

namespace Beyond8.Integration.Application.Helpers.AiService
{
    public static class SubscriptionHelper
    {
        public static async Task<(bool IsAllowed, string Message, object? Metadata)> CheckSubscriptionStatusAsync(
                    IIdentityClient identityClient,
                    Guid userId)
        {
            try
            {
                var response = await identityClient.GetUserSubscriptionAsync(userId);

                if (!response.IsSuccess || response.Data == null)
                {
                    return (false, "Không thể lấy thông tin gói đăng ký hoặc dữ liệu trống.", null);
                }

                var data = response.Data;
                var planName = data.SubscriptionPlan?.Name;

                if (!data.IsRequestLimitedReached)
                {
                    return (true, "Gói đăng ký hợp lệ.", null);
                }

                bool isFreePlan = string.Equals(planName, "FREE", StringComparison.OrdinalIgnoreCase);

                if (isFreePlan)
                {
                    return (false, "Bạn đã hết số lần sử dụng AI miễn phí. Vui lòng đăng ký gói dịch vụ để tiếp tục.", null);
                }

                return (false,
                    $"Bạn đã hết số lần sử dụng AI với gói {planName}. Vui lòng gia hạn hoặc mua thêm.",
                    new { RecoverTime = data.RequestLimitedEndsAt });
            }
            catch (Exception ex)
            {
                return (false, "Lỗi hệ thống khi kiểm tra quyền truy cập.", new { ErrorDetail = ex.Message });
            }
        }
    }
}