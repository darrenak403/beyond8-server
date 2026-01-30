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

                var notifications = result.Items.Select(n => n.ToNotificationResponse(userId)).ToList();

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

        public async Task<ApiResponse<InstructorNotificationResponse>> GetInstructorNotificationsAsync(
            Guid userId,
            PaginationNotificationRequest pagination)
        {
            try
            {
                // User notifications (AllUser)
                var userTargets = new List<NotificationTarget> { NotificationTarget.AllUser };
                var userResult = await unitOfWork.NotificationRepository.GetNotificationsByUserAndRolesAsync(
                    userId,
                    userTargets,
                    pagination.PageNumber,
                    pagination.PageSize,
                    pagination.Status,
                    pagination.Channel,
                    pagination.IsRead);

                // Instructor notifications (AllInstructor)
                var instructorTargets = new List<NotificationTarget> { NotificationTarget.AllInstructor };
                var instructorResult = await unitOfWork.NotificationRepository.GetNotificationsByUserAndRolesAsync(
                    userId,
                    instructorTargets,
                    pagination.PageNumber,
                    pagination.PageSize,
                    pagination.Status,
                    pagination.Channel,
                    pagination.IsRead);

                var response = new InstructorNotificationResponse
                {
                    UserNotifications = new NotificationSection
                    {
                        Items = [.. userResult.Items.Select(n => n.ToNotificationResponse(userId)).ToList()],
                        TotalCount = userResult.TotalCount,
                        PageNumber = pagination.PageNumber,
                        PageSize = pagination.PageSize
                    },
                    InstructorNotifications = new NotificationSection
                    {
                        Items = [.. instructorResult.Items.Select(n => n.ToNotificationResponse(userId)).ToList()],
                        TotalCount = instructorResult.TotalCount,
                        PageNumber = pagination.PageNumber,
                        PageSize = pagination.PageSize
                    }
                };

                logger.LogInformation("Retrieved {UserCount} user notifications and {InstructorCount} instructor notifications for user {UserId}",
                    response.UserNotifications.Items.Count, response.InstructorNotifications.Items.Count, userId);

                return ApiResponse<InstructorNotificationResponse>.SuccessResponse(
                    response,
                    "Lấy danh sách thông báo thành công.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving instructor notifications for user {UserId}", userId);
                return ApiResponse<InstructorNotificationResponse>.FailureResponse(
                    "Đã xảy ra lỗi khi lấy danh sách thông báo.");
            }
        }

        private static List<NotificationTarget> GetAllowedTargets(List<string> userRoles)
        {
            var targets = new List<NotificationTarget> { NotificationTarget.AllUser };

            if (userRoles.Contains(Role.Admin))
            {
                targets.Add(NotificationTarget.AllAdmin);
            }
            else if (userRoles.Contains(Role.Staff))
            {
                targets.Add(NotificationTarget.AllStaff);
            }
            else if (userRoles.Contains(Role.Instructor))
            {
                targets.Add(NotificationTarget.AllInstructor);
            }

            return targets;
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

        public async Task<ApiResponse<bool>> ReadAllNotificationAsync(Guid userId)
        {
            try
            {
                var notifications = await unitOfWork.NotificationRepository.GetAllAsync(n => n.UserId == userId);
                foreach (var n in notifications)
                {
                    n.IsRead = true;
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

        public async Task<ApiResponse<bool>> DeleteAllNotificationAsync(Guid userId)
        {
            try
            {
                var notifications = await unitOfWork.NotificationRepository.GetAllAsync(n => n.UserId == userId);
                foreach (var n in notifications)
                {
                    n.Status = NotificationStatus.Deleted;
                    await unitOfWork.NotificationRepository.UpdateAsync(n.Id, n);
                }
                await unitOfWork.SaveChangesAsync();
                return ApiResponse<bool>.SuccessResponse(true, "Xóa tất cả thông báo thành công.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting all notifications for user {UserId}", userId);
                return ApiResponse<bool>.FailureResponse("Đã xảy ra lỗi khi xóa tất cả thông báo.");
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


        public async Task<ApiResponse<NotificationStatusResponse>> GetNotificationStatusAsync(Guid userId)
        {
            try
            {
                var notifications = await unitOfWork.NotificationRepository.GetAllAsync(n => n.UserId == userId && n.IsRead == false);
                return ApiResponse<NotificationStatusResponse>.SuccessResponse(new NotificationStatusResponse { IsRead = notifications.Count > 0, UnreadCount = notifications.Count }, "Lấy trạng thái thông báo thành công.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting notification status for user {UserId}", userId);
                return ApiResponse<NotificationStatusResponse>.FailureResponse("Đã xảy ra lỗi khi lấy trạng thái thông báo.");
            }
        }
    }
}