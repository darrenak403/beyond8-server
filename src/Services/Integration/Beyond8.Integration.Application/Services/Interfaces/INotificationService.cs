using Beyond8.Integration.Application.Dtos.Notifications;

namespace Beyond8.Integration.Application.Services.Interfaces
{
    public interface INotificationService
    {
        Task SendToUserAsync(string userId, string method, DataInfor data);
        Task SendToAllUserAsync(string method, DataInfor data);
        Task SendToGroupAsync(string groupName, string method, DataInfor data);
    }
}
