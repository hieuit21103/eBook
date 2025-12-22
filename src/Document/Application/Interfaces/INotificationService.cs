namespace Application.Interfaces;

public interface INotificationService
{
    Task NotifyPageCreatedAsync(Guid documentId, Guid pageId);
    Task NotifyPageDeletedAsync(Guid documentId, Guid pageId);
    Task NotifyPageUpdatedAsync(Guid documentId, Guid pageId);
    Task SendAsync(string group, string method, object? args = null);
}