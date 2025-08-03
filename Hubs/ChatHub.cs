using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ChatApp.Api.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private const string GlobalRoom = "global";

    public override async Task OnConnectedAsync()
    {
        var username = Context.User?.Identity?.Name ?? "Anonymous";
        await Groups.AddToGroupAsync(Context.ConnectionId, GlobalRoom);
        await Clients.Group(GlobalRoom).SendAsync("UserJoined", username);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var username = Context.User?.Identity?.Name ?? "Anonymous";
        await Clients.Group(GlobalRoom).SendAsync("UserLeft", username);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, GlobalRoom);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(string message)
    {
        var username = Context.User?.Identity?.Name ?? "Anonymous";
        await Clients.Group(GlobalRoom).SendAsync("ReceiveMessage", username, message);
    }
}
