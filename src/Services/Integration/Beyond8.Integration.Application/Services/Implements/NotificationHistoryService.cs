using Beyond8.Common.Security;
using Beyond8.Common.Utilities;
using Beyond8.Integration.Application.Dtos.Notifications;
using Beyond8.Integration.Application.Services.Interfaces;
using Beyond8.Integration.Domain.Enums;
using Beyond8.Integration.Domain.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace Beyond8.Integration.Application.Services.Implements;

public class NotificationHistoryService(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    ILogger<NotificationHistoryService> logger
) : INotificationHistoryService
{
    public async Task<ApiResponse<List<NotificationResponse>>> GetMyNotificationsAsync(
        Guid userId,
        List<string> userRoles,
        PaginationNotificationRequest pagination)
    {
        try
        {
            // Determine allowed targets based on user roles
            var allowedTargets = GetAllowedTargets(userRoles);

            var result = await unitOfWork.NotificationRepository.GetNotificationsByUserAndRolesAsync(
                userId,
                allowedTargets,
                pagination.PageNumber,
                pagination.PageSize,
                pagination.Status,
                pagination.Channel,
                pagination.IsRead);

            var notifications = result.Items.Select(n => new NotificationResponse
            {
                Id = n.Id,
                Title = n.Title,
                Message = n.Message,
                UserId = n.UserId == Guid.Empty ? userId : n.UserId,
                Target = n.Target,
                Status = n.Status,
                Channels = n.Channels,
                ReadAt = n.ReadAt,
                IsRead = n.IsRead
            }).ToList();

            logger.LogInformation("Retrieved {Count} notifications for user {UserId} with roles {Roles}",
                notifications.Count, userId, string.Join(", ", userRoles));

            return ApiResponse<List<NotificationResponse>>.SuccessPagedResponse(
                notifications,
                result.TotalCount,
                pagination.PageNumber,
                pagination.PageSize,
                "Lấy danh sách thông báo thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving notifications for user {UserId}", userId);
            return ApiResponse<List<NotificationResponse>>.FailureResponse(
                "Đã xảy ra lỗi khi lấy danh sách thông báo.");
        }
    }

    private static List<NotificationTarget> GetAllowedTargets(List<string> userRoles)
    {
        var targets = new List<NotificationTarget> { NotificationTarget.AllUser };

        if (userRoles.Contains(Role.Admin))
        {
            targets.AddRange([
                NotificationTarget.AllAdmin,
                NotificationTarget.AllStaff,
                NotificationTarget.AllInstructor
            ]);
        }
        else if (userRoles.Contains(Role.Staff))
        {
            targets.AddRange([
                NotificationTarget.AllStaff,
                NotificationTarget.AllAdmin
            ]);
        }
        else if (userRoles.Contains(Role.Instructor))
        {
            targets.Add(NotificationTarget.AllInstructor);
        }

        return targets;
    }
}
