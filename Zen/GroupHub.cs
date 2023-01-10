using Microsoft.AspNetCore.SignalR;
namespace Zen;

public class GroupHub : Hub
{
    public Task Join() => Groups.AddToGroupAsync(Context.ConnectionId, "group_message");

    public Task Leave() => Groups.RemoveFromGroupAsync(Context.ConnectionId, "group_message");

    public Task Message() => Clients
        .Groups("group_message")
        .SendAsync("group_message", new Data(69, "message to group_message"));
}
