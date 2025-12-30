using Infrastructure.Hubs;
using Infrastructure.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Documents.Test.Services;

public class SignalRServiceTests
{
    private readonly IHubContext<DocumentHub> _hubContext;
    private readonly ILogger<SignalRService> _logger;
    private readonly SignalRService _service;
    private readonly IHubClients _hubClients;
    private readonly IClientProxy _clientProxy;

    public SignalRServiceTests()
    {
        _hubContext = Substitute.For<IHubContext<DocumentHub>>();
        _hubClients = Substitute.For<IHubClients>();
        _clientProxy = Substitute.For<IClientProxy>();
        _logger = Substitute.For<ILogger<SignalRService>>();

        _hubContext.Clients.Returns(_hubClients);
        _hubClients.Group(Arg.Any<string>()).Returns(_clientProxy);

        _service = new SignalRService(_hubContext, _logger);
    }

    [Fact]
    public async Task NotifyPageCreatedAsync_ShouldSendSignalRMessage()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var pageId = Guid.NewGuid();
        var groupName = $"document-{documentId}";

        // Act
        await _service.NotifyPageCreatedAsync(documentId, pageId);

        // Assert
        _hubClients.Received(1).Group(groupName);
        await _clientProxy.Received(1).SendCoreAsync("PageCreated", 
            Arg.Is<object[]>(args => args.Length == 1 && args[0] != null && args[0].ToString()!.Contains(pageId.ToString())), 
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task NotifyPageDeletedAsync_ShouldSendSignalRMessage()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var pageId = Guid.NewGuid();
        var groupName = $"document-{documentId}";

        // Act
        await _service.NotifyPageDeletedAsync(documentId, pageId);

        // Assert
        _hubClients.Received(1).Group(groupName);
        await _clientProxy.Received(1).SendCoreAsync("PageDeleted", 
            Arg.Is<object[]>(args => args.Length == 1 && args[0] != null && args[0].ToString()!.Contains(pageId.ToString())), 
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task NotifyPageUpdatedAsync_ShouldSendSignalRMessage()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var pageId = Guid.NewGuid();
        var groupName = $"document-{documentId}";

        // Act
        await _service.NotifyPageUpdatedAsync(documentId, pageId);

        // Assert
        _hubClients.Received(1).Group(groupName);
        await _clientProxy.Received(1).SendCoreAsync("PageUpdated", 
            Arg.Is<object[]>(args => args.Length == 1 && args[0] != null && args[0].ToString()!.Contains(pageId.ToString())), 
            Arg.Any<CancellationToken>());
    }
}
