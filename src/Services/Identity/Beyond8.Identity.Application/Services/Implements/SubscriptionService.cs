using Beyond8.Common.Utilities;
using Beyond8.Identity.Application.Dtos.Users;
using Beyond8.Identity.Application.Mappings.SubscriptionMappings;
using Beyond8.Identity.Application.Services.Interfaces;
using Beyond8.Identity.Domain.Enums;
using Beyond8.Identity.Domain.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace Beyond8.Identity.Application.Services.Implements;

public class SubscriptionService(
    ILogger<SubscriptionService> logger,
    IUnitOfWork unitOfWork) : ISubscriptionService
{
    public async Task<ApiResponse<List<SubscriptionResponse>>> GetAllSubscriptionsAsync()
    {
        try
        {
            var subscriptions = await unitOfWork.UserSubscriptionRepository.GetActiveSubscriptionsWithPlanAsync();
            return ApiResponse<List<SubscriptionResponse>>.SuccessResponse([.. subscriptions.Select(s => s.ToSubscriptionResponse())], "Lấy danh sách gói đăng ký thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting all subscriptions");
            return ApiResponse<List<SubscriptionResponse>>.FailureResponse("Đã xảy ra lỗi khi lấy danh sách gói đăng ký.");
        }
    }

    public async Task<ApiResponse<SubscriptionResponse>> GetMySubscriptionStatsAsync(Guid userId)
    {
        try
        {
            var subscription = await unitOfWork.UserSubscriptionRepository.GetActiveByUserIdAsync(userId);
            if (subscription == null)
            {
                logger.LogWarning("User {UserId} has no active subscription", userId);
                return ApiResponse<SubscriptionResponse>.FailureResponse("Người dùng không có gói đăng ký hoạt động.");
            }
            return ApiResponse<SubscriptionResponse>.SuccessResponse(subscription.ToSubscriptionResponse(), "Lấy thông tin gói đăng ký thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting my subscription stats for user {UserId}", userId);
            return ApiResponse<SubscriptionResponse>.FailureResponse("Đã xảy ra lỗi khi lấy thông tin gói đăng ký.");
        }
    }

    public async Task<ApiResponse<List<SubscriptionPlanResponse>>> GetSubscriptionPlansAsync()
    {
        try
        {
            var subscriptionPlans = await unitOfWork.SubscriptionPlanRepository.GetAllAsync();
            var orderedSubscriptionPlans = subscriptionPlans.OrderBy(p => p.Price).ToList();
            if (orderedSubscriptionPlans == null)
            {
                logger.LogWarning("No subscription plans found");
                return ApiResponse<List<SubscriptionPlanResponse>>.FailureResponse("Không tìm thấy gói đăng ký.");
            }
            return ApiResponse<List<SubscriptionPlanResponse>>.SuccessResponse([.. orderedSubscriptionPlans.Select(p => p.ToSubscriptionPlanResponse())], "Lấy danh sách gói đăng ký thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting subscription plans");
            return ApiResponse<List<SubscriptionPlanResponse>>.FailureResponse("Đã xảy ra lỗi khi lấy danh sách gói đăng ký.");
        }
    }

    public async Task<ApiResponse<SubscriptionResponse>> UpdateSubscriptionAsync(Guid userId, UpdateUsageQuotaRequest request)
    {
        try
        {
            var subscription = await unitOfWork.UserSubscriptionRepository.GetActiveByUserIdAsync(userId);
            if (subscription == null)
            {
                logger.LogWarning("User {UserId} has no active subscription", userId);
                return ApiResponse<SubscriptionResponse>.FailureResponse("Người dùng không có gói đăng ký hoạt động.");
            }
            subscription.UpdateUsageQuotaRequest(request);
            await unitOfWork.UserSubscriptionRepository.UpdateAsync(subscription.Id, subscription);
            await unitOfWork.SaveChangesAsync();
            return ApiResponse<SubscriptionResponse>.SuccessResponse(subscription.ToSubscriptionResponse(), "Cập nhật gói đăng ký thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating subscription for user {UserId}", userId);
            return ApiResponse<SubscriptionResponse>.FailureResponse("Đã xảy ra lỗi khi cập nhật gói đăng ký.");
        }
    }

    public async Task ResetWeeklyRequestsAsync()
    {
        try
        {
            var subscriptions = await unitOfWork.UserSubscriptionRepository.GetActiveSubscriptionsWithPlanAsync();
            foreach (var subscription in subscriptions)
            {
                subscription.RemainingRequestsPerWeek = subscription.Plan?.MaxRequestsPerWeek ?? subscription.RemainingRequestsPerWeek;
                subscription.RequestLimitedEndsAt = null;
            }
            await unitOfWork.SaveChangesAsync();
            logger.LogInformation("Reset weekly requests completed. Updated {Count} active subscriptions.", subscriptions.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error resetting weekly requests for active subscriptions.");
            throw;
        }
    }

    public async Task ExpireSubscriptionsAsync()
    {
        try
        {
            var subscriptions = await unitOfWork.UserSubscriptionRepository.GetActiveSubscriptionsWhereExpiredAsync();
            foreach (var subscription in subscriptions)
            {
                subscription.Status = SubscriptionStatus.Expired;
            }
            await unitOfWork.SaveChangesAsync();
            logger.LogInformation("Expired {Count} subscriptions.", subscriptions.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error expiring subscriptions.");
            throw;
        }
    }
}
