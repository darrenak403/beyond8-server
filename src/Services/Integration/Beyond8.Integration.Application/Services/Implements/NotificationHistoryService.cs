using Beyond8.Common.Utilities;
using Beyond8.Integration.Application.Dtos.Notifications;
using Beyond8.Integration.Application.Mappings.NotificationMappings;
using Beyond8.Integration.Application.Services.Interfaces;
using Beyond8.Integration.Domain.Enums;
using Beyond8.Integration.Domain.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace Beyond8.Integration.Application.Services.Implements
{
    public class NotificationHistoryService(
        IUnitOfWork unitOfWork,
        ILogger<NotificationHistoryService> logger
    ) : INotificationHistoryService
    {
        public async Task<ApiResponse<List<NotificationResponse>>> GetNotificationsByContextAsync(
            Guid userId,
            NotificationContext context,
            PaginationNotificationRequest pagination)
        {
            try
            {
                var result = await unitOfWork.NotificationRepository.GetNotificationsByContextAsync(
                    userId,
                    context,
                    pagination.PageNumber,
                    pagination.PageSize,
                    pagination.Status,
                    pagination.Channel,
                    pagination.IsRead);

                var notifications = result.Items.Select(n => n.ToNotificationResponse(userId)).ToList();

                logger.LogInformation("Retrieved {Count} notifications for user {UserId} in context {Context}",
                    notifications.Count, userId, context);

                return ApiResponse<List<NotificationResponse>>.SuccessPagedResponse(
                    notifications,
                    result.TotalCount,
                    pagination.PageNumber,
                    pagination.PageSize,
                    "Lấy danh sách thông báo thành công.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving notifications for user {UserId} in context {Context}", userId, context);
                return ApiResponse<List<NotificationResponse>>.FailureResponse(
                    "Đã xảy ra lỗi khi lấy danh sách thông báo.");
            }
        }

        public async Task<ApiResponse<bool>> UnreadNotificationAsync(Guid id, Guid userId)
        {
            try
            {
                var notification = await unitOfWork.NotificationRepository.FindOneAsync(n => n.Id == id && n.UserId == userId);
                if (notification == null)
                {
                    return ApiResponse<bool>.FailureResponse("Thông báo không tồn tại");
                }
                notification.IsRead = false;
                notification.ReadAt = null;
                await unitOfWork.NotificationRepository.UpdateAsync(notification.Id, notification);
                await unitOfWork.SaveChangesAsync();
                return ApiResponse<bool>.SuccessResponse(true, "Đánh dấu thông báo chưa đọc thành công.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error unreading notification {Id} for user {UserId}", id, userId);
                return ApiResponse<bool>.FailureResponse("Đã xảy ra lỗi khi đánh dấu thông báo chưa đọc.");
            }
        }

        public async Task<ApiResponse<bool>> ReadNotificationAsync(Guid id, Guid userId)
        {
            try
            {
                var notification = await unitOfWork.NotificationRepository.FindOneAsync(n => n.Id == id && n.UserId == userId);
                if (notification == null)
                {
                    return ApiResponse<bool>.FailureResponse("Thông báo không tồn tại");
                }
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
                await unitOfWork.NotificationRepository.UpdateAsync(notification.Id, notification);
                await unitOfWork.SaveChangesAsync();
                return ApiResponse<bool>.SuccessResponse(true, "Đánh dấu thông báo đã đọc thành công.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error reading notification for user {UserId}", userId);
                return ApiResponse<bool>.FailureResponse("Đã xảy ra lỗi khi đánh dấu thông báo đã đọc.");
            }
        }

        public async Task<ApiResponse<bool>> ReadAllNotificationAsync(Guid userId, NotificationContext context)
        {
            try
            {
                var notifications = await unitOfWork.NotificationRepository.GetAllAsync(n =>
                    n.UserId == userId &&
                    (n.Context == context || n.Context == NotificationContext.General));

                foreach (var n in notifications)
                {
                    n.IsRead = true;
                    n.ReadAt = DateTime.UtcNow;
                    await unitOfWork.NotificationRepository.UpdateAsync(n.Id, n);
                }
                await unitOfWork.SaveChangesAsync();
                return ApiResponse<bool>.SuccessResponse(true, "Đánh dấu tất cả thông báo đã đọc thành công.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error reading all notifications for user {UserId}", userId);
                return ApiResponse<bool>.FailureResponse("Đã xảy ra lỗi khi đánh dấu tất cả thông báo đã đọc.");
            }
        }

        public async Task<ApiResponse<bool>> DeleteNotificationAsync(Guid id, Guid userId)
        {
            try
            {
                var notification = await unitOfWork.NotificationRepository.FindOneAsync(n => n.Id == id && n.UserId == userId);
                if (notification == null)
                {
                    return ApiResponse<bool>.FailureResponse("Thông báo không tồn tại");
                }
                notification.Status = NotificationStatus.Deleted;
                notification.DeletedAt = DateTime.UtcNow;
                notification.DeletedBy = userId;
                await unitOfWork.NotificationRepository.UpdateAsync(notification.Id, notification);
                await unitOfWork.SaveChangesAsync();
                return ApiResponse<bool>.SuccessResponse(true, "Xóa thông báo thành công.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting notification {Id} for user {UserId}", id, userId);
                return ApiResponse<bool>.FailureResponse("Đã xảy ra lỗi khi xóa thông báo.");
            }
        }

        public async Task<ApiResponse<NotificationStatusResponse>> GetNotificationStatusAsync(Guid userId, NotificationContext context)
        {
            try
            {
                var unreadCount = await unitOfWork.NotificationRepository.GetUnreadCountByContextAsync(userId, context);

                var response = new NotificationStatusResponse
                {
                    IsRead = unreadCount == 0,
                    UnreadCount = unreadCount
                };

                return ApiResponse<NotificationStatusResponse>.SuccessResponse(response, "Lấy trạng thái thông báo thành công.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting notification status for user {UserId}", userId);
                return ApiResponse<NotificationStatusResponse>.FailureResponse("Đã xảy ra lỗi khi lấy trạng thái thông báo.");
            }
        }

        public async Task<ApiResponse<List<NotificationResponse>>> GetStaffNotificationsAsync(
            Guid userId,
            PaginationNotificationRequest pagination)
        {
            try
            {
                var result = await unitOfWork.NotificationRepository.GetNotificationsByTargetAsync(
                    NotificationTarget.AllStaff,
                    pagination.PageNumber,
                    pagination.PageSize,
                    pagination.Status,
                    pagination.IsRead);

                var notifications = result.Items.Select(n => n.ToNotificationResponse(userId)).ToList();

                logger.LogInformation("Retrieved {Count} staff notifications", notifications.Count);

                return ApiResponse<List<NotificationResponse>>.SuccessPagedResponse(
                    notifications,
                    result.TotalCount,
                    pagination.PageNumber,
                    pagination.PageSize,
                    "Lấy danh sách thông báo thành công.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving staff notifications");
                return ApiResponse<List<NotificationResponse>>.FailureResponse(
                    "Đã xảy ra lỗi khi lấy danh sách thông báo.");
            }
        }

        public async Task<ApiResponse<NotificationStatusResponse>> GetStaffNotificationStatusAsync(Guid userId)
        {
            try
            {
                var unreadCount = await unitOfWork.NotificationRepository.GetUnreadCountByTargetAsync(NotificationTarget.AllStaff);

                var response = new NotificationStatusResponse
                {
                    IsRead = unreadCount == 0,
                    UnreadCount = unreadCount
                };

                return ApiResponse<NotificationStatusResponse>.SuccessResponse(response, "Lấy trạng thái thông báo thành công.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting staff notification status");
                return ApiResponse<NotificationStatusResponse>.FailureResponse("Đã xảy ra lỗi khi lấy trạng thái thông báo.");
            }
        }

        public async Task<ApiResponse<List<NotificationLogResponse>>> GetAllNotificationLogsAsync(
            PaginationNotificationRequest pagination)
        {
            try
            {
                var result = await unitOfWork.NotificationRepository.GetAllNotificationsAsync(
                    pagination.PageNumber,
                    pagination.PageSize,
                    pagination.Status,
                    pagination.Channel,
                    pagination.IsRead);

                // Convert to log response (without Title and Message for privacy)
                var logs = result.Items.Select(n => n.ToNotificationLogResponse()).ToList();

                logger.LogInformation("Retrieved {Count} notification logs for admin", logs.Count);

                return ApiResponse<List<NotificationLogResponse>>.SuccessPagedResponse(
                    logs,
                    result.TotalCount,
                    pagination.PageNumber,
                    pagination.PageSize,
                    "Lấy danh sách log thông báo thành công.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving notification logs");
                return ApiResponse<List<NotificationLogResponse>>.FailureResponse(
                    "Đã xảy ra lỗi khi lấy danh sách log thông báo.");
            }
        }
    }
}
