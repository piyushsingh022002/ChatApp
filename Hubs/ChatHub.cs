using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ChatApp.Api.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private const string GlobalRoom = "global";

    public override async Task OnConnectedAsync()
{
    var name = Context.User?.Identity?.Name ?? "unauthenticated";
    Console.WriteLine($"[SignalR] Connected User: {name}");

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
        await Clients.All.SendAsync("ReceiveMessage", username, message);
    }
}
