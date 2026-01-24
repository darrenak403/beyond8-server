using Beyond8.Integration.Application.Services.Interfaces;
using Beyond8.Integration.Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Beyond8.Integration.Infrastructure.ExternalServices.Hubs.SingalR;

public class SignalRNotificationService : INotificationService
{
    public SignalRNotificationService(IHubContext<AppHub> hubContext)
    {
        _hubContext = hubContext;
    }

    private readonly IHubContext<AppHub> _hubContext;

    public Task SendToUserAsync(string userId, string method, object data)
    {
        return _hubContext.Clients.User(userId).SendAsync(method, data);
    }

    public Task SendToAllUserAsync(string method, object data)
    {
        return _hubContext.Clients.All.SendAsync(method, data);
    }

    public Task SendToGroupAsync(string groupName, string method, object data)
    {
        return _hubContext.Clients.Group(groupName).SendAsync(method, data);
    }
}
