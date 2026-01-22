using System;

namespace Beyond8.Integration.Application.Services.Interfaces;

public interface INotificationService
{
    Task SendToUserAsync(string userId, string method, object data);
    Task SendToAllUserAsync(string method, object data);
    Task SendToGroupAsync(string groupName, string method, object data);
}
