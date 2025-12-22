using MassTransit;
using Shared;
using FileStorage.Application.Interfaces;

namespace FileStorage.IntegrationEvents
{
    public class PageDeletedConsumer : IConsumer<PageDeletedEvent>
    {
        private readonly IFileStorageService _fileStorageService;
        public PageDeletedConsumer(IFileStorageService fileStorageService)
        {
            _fileStorageService = fileStorageService;
        }
        public async Task Consume(ConsumeContext<PageDeletedEvent> context)
        {
            var id = context.Message.FileId;
            if (id != null)
            {
                await _fileStorageService.DeleteFileAsync(id.Value);
            }
        }
    }
}