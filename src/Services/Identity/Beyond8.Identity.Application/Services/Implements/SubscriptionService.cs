using System;
using Beyond8.Common.Utilities;
using Beyond8.Identity.Application.Dtos.Users;
using Beyond8.Identity.Application.Mappings.SubscriptionMappings;
using Beyond8.Identity.Application.Services.Interfaces;
using Beyond8.Identity.Domain.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace Beyond8.Identity.Application.Services.Implements;

public class SubscriptionService(
    ILogger<SubscriptionService> logger,
    IUnitOfWork unitOfWork) : ISubscriptionService
{
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
}
