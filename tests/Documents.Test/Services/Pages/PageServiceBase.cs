using Domain.Interfaces;
using Application.Services;
using Domain.Entities;
using FileStorage.Protos;
using MassTransit;
using Application.Interfaces;
using NSubstitute;

namespace Documents.Test.Services.Pages;

public abstract class PageServiceBase
{
    protected readonly IPageRepository _pageRepository;
    protected readonly IDocumentRepository _documentRepository;
    protected readonly FileStorageService.FileStorageServiceClient _grpcClient;
    protected readonly INotificationService _notificationService;
    protected readonly IPublishEndpoint _publishEndpoint;
    protected readonly PageService _pageService;

    protected PageServiceBase()
    {
        _pageRepository = Substitute.For<IPageRepository>();
        _documentRepository = Substitute.For<IDocumentRepository>();
        _grpcClient = Substitute.For<FileStorageService.FileStorageServiceClient>();
        _notificationService = Substitute.For<INotificationService>();
        _publishEndpoint = Substitute.For<IPublishEndpoint>();
        _pageService = new PageService(_pageRepository, _documentRepository, _grpcClient, _notificationService, _publishEndpoint);
    }
}
