using MassTransit;
using Shared;
using FileStorage.Application.Interfaces;

namespace FileStorage.IntegrationEvents
{
    public class PageUpdatedConsumer : IConsumer<PageUpdatedEvent>
    {
        private readonly IFileStorageService _fileStorageService;
        public PageUpdatedConsumer(IFileStorageService fileStorageService)
        {
            _fileStorageService = fileStorageService;
        }
        public async Task Consume(ConsumeContext<PageUpdatedEvent> context)
        {
            var id = context.Message.OldFileId;
            if (id != null)
            {
                await _fileStorageService.DeleteFileAsync(id.Value);
            }
        }
    }
}