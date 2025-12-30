using Microsoft.AspNetCore.SignalR;

namespace Infrastructure.Hubs;

public class DocumentHub : Hub
{
    public async Task JoinDocumentGroup(string documentId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"document_{documentId}");
    }

    public async Task LeaveDocumentGroup(string documentId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"document_{documentId}");
    }

    public string GetConnectionId()
    {
        return Context.ConnectionId;
    }

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }

}
