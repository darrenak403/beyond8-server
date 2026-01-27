using Beyond8.Common.Utilities;
using Beyond8.Integration.Application.Dtos.Notifications;
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
                        Items = [.. userResult.Items.Select(n => new NotificationResponse
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
                        })],
                        TotalCount = userResult.TotalCount,
                        PageNumber = pagination.PageNumber,
                        PageSize = pagination.PageSize
                    },
                    InstructorNotifications = new NotificationSection
                    {
                        Items = [.. instructorResult.Items.Select(n => new NotificationResponse
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
                        })],
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
    }
}
