using Shared;
using MassTransit;
using Application.Interfaces;

namespace IntegrationEvents;

public class UserStatusChangedConsumer : IConsumer<UserStatusChangedEvent>
{
    private readonly ILogger<UserStatusChangedConsumer> _logger;
    private readonly IDocumentService _documentService;
    public UserStatusChangedConsumer(ILogger<UserStatusChangedConsumer> logger, IDocumentService documentService)
    {
        _logger = logger;
        _documentService = documentService;
    }
    public async Task Consume(ConsumeContext<UserStatusChangedEvent> context)
    {
        var userId = context.Message.UserId;
        var isActive = context.Message.IsActive;
        if (!isActive)
        {
            _logger.LogInformation("User with ID {UserId} has been deactivated. Deleting all documents associated with this user.", userId);
            await _documentService.DeleteDocumentsByUserIdAsync(userId);
        }
        else
        {
            _logger.LogInformation("User with ID {UserId} has been activated. Restoring all documents associated with this user.", userId);
            await _documentService.RestoreDocumentsByUserIdAsync(userId);
        }
    }
}