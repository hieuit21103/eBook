using Microsoft.AspNetCore.SignalR;
using Application.Interfaces;
using Infrastructure.Hubs;
namespace Infrastructure.Services;

public class SignalRService : INotificationService
{
    private readonly IHubContext<DocumentHub> _hubContext;
    private readonly ILogger<SignalRService> _logger;

    public SignalRService(
        IHubContext<DocumentHub> hubContext,
        ILogger<SignalRService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task NotifyPageCreatedAsync(Guid documentId, Guid pageId)
    {
        await SendAsync(
            group: $"document-{documentId}",
            method: "PageCreated",
            args: new { PageId = pageId, DocumentId = documentId });
    }

    public async Task NotifyPageDeletedAsync(Guid documentId, Guid pageId)
    {
        await SendAsync(
            group: $"document-{documentId}",
            method: "PageDeleted",
            args: new { PageId = pageId, DocumentId = documentId });
    }

    public async Task NotifyPageUpdatedAsync(Guid documentId, Guid pageId)
    {
        await SendAsync(
            group: $"document-{documentId}",
            method: "PageUpdated",
            args: new { PageId = pageId, DocumentId = documentId });
    }

    public async Task SendAsync(string group, string method, object? args = null)
    {
        await _hubContext.Clients.Group(group).SendAsync(method, args);
        _logger.LogInformation("Sent SignalR message to group {Group} with method {Method}", group, method);
    }
}