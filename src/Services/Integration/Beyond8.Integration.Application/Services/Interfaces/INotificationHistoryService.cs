using Beyond8.Common.Utilities;
using Beyond8.Integration.Application.Dtos.Notifications;

namespace Beyond8.Integration.Application.Services.Interfaces
{
    public interface INotificationHistoryService
    {
        Task<ApiResponse<List<NotificationResponse>>> GetMyNotificationsAsync(
            Guid userId,
            List<string> userRoles,
            PaginationNotificationRequest pagination);

        Task<ApiResponse<InstructorNotificationResponse>> GetInstructorNotificationsAsync(
            Guid userId,
            PaginationNotificationRequest pagination);
        Task<ApiResponse<bool>> UnreadNotificationAsync(Guid id, Guid userId);
        Task<ApiResponse<bool>> ReadNotificationAsync(Guid userId);
    }
}
