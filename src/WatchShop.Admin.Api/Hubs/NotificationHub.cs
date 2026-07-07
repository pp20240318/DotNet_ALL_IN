using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace WatchShop.Admin.Api.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    public const string AdminGroup = "admin-notifications";

    public override async Task OnConnectedAsync()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, AdminGroup);
        await base.OnConnectedAsync();
    }
}
