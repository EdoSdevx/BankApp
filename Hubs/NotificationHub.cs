using System.Security.Claims;
using BankApp.BankApp.Common.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace BankApp.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        if (Context.User?.IsInRole(RoleNames.Admin) == true ||
            Context.User?.IsInRole(RoleNames.Employee) == true)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
        }

        await base.OnConnectedAsync();
    }
}
