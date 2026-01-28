using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Beyond8.Integration.Infrastructure.Hubs
{
    [Authorize]
    public class AppHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var roles = Context.User?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

            if (roles != null && roles.Count != 0)
            {
                foreach (var role in roles)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"{role}Group");
                }
            }

            await base.OnConnectedAsync();
        }
    }
}
