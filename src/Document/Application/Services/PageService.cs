using Application.DTOs.Page;
using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Filters;
using Application.DTOs;
using Shared.DTOs;
using Grpc.Core;
using FileStorage.Protos;
using Google.Protobuf;
using MassTransit;  
using Shared;


namespace Application.Services;

public class PageService : IPageService
{
    private readonly IPageRepository _pageRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly FileStorageService.FileStorageServiceClient _grpcClient;
    private readonly INotificationService _notificationService;
    private readonly IPublishEndpoint _publishEndpoint;

    public PageService(
        IPageRepository pageRepository,
        IDocumentRepository documentRepository,
        FileStorageService.FileStorageServiceClient grpcClient,
        INotificationService notificationService,
        IPublishEndpoint publishEndpoint
    )
    {
        _pageRepository = pageRepository;
        _documentRepository = documentRepository;
        _grpcClient = grpcClient;
        _notificationService = notificationService;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<PageResponse> GetByIdAsync(Guid id)
    {
        var page = await _pageRepository.GetByIdWithDetailsAsync(id);

        if (page == null)
        {
            throw new KeyNotFoundException($"Page with ID {id} not found.");
        }

        return MapToResponse(page);
    }

    public async Task<PagedResult<PageResponse>> GetAllAsync(PageFilterParams filterParams)
    {
        var pagedResult = await _pageRepository.GetPagedAsync(filterParams);

        var pageResponses = pagedResult.Items.Select(MapToResponse).ToList();

        return new PagedResult<PageResponse>
        {
            Items = pageResponses,
            CurrentPage = pagedResult.CurrentPage,
            PageSize = pagedResult.PageSize,
            TotalPages = pagedResult.TotalPages,
            TotalCount = pagedResult.TotalCount
        };
    }

    public async Task<IEnumerable<PageResponse>> GetPagesByDocumentIdAsync(Guid documentId, PageFilterParams filterParams)
    {
        var pages = await _pageRepository.GetPagesByDocumentIdAsync(documentId);
        return pages.Select(MapToResponse);
    }

    public async Task<PageResponse> GetByDocumentIdAndPageNumberAsync(Guid documentId, int pageNumber)
    {
        var page = await _pageRepository.GetByDocumentIdAndPageNumberAsync(documentId, pageNumber);
        if (page == null)
        {
            throw new KeyNotFoundException(
                $"Page number {pageNumber} not found for document ID {documentId}.");
        }
        return MapToResponse(page);
    }

    public async Task<PagePreviewResponse> GetPreviewByDocumentIdAndPageNumberAsync(Guid documentId, int pageNumber)
    {
        var page = await _pageRepository.GetByDocumentIdAndPageNumberAsync(documentId, pageNumber);
        if (page == null)
        {
            throw new KeyNotFoundException(
                $"Page number {pageNumber} not found for document ID {documentId}.");
        }
        
        var response = _grpcClient.GetPresignedUrl(new FileRequest
        {
            Id = page.FileId.ToString(),
        });
        return new PagePreviewResponse
        {
            Id = page.Id,
            DocumentId = page.DocumentId,
            PageNumber = page.PageNumber,
            FileType = response.FileType,
            Url = response.Url
        };
    }

    public async Task<PageDownloadResponse> DownloadContentAsync(Guid id)
    {
        var page = await _pageRepository.GetByIdAsync(id);
        if (page == null)
        {
            throw new KeyNotFoundException(
                $"Page with ID {id} not found.");
        }

        using var call = _grpcClient.DownloadFile(new FileRequest
        {
            Id = page.FileId.ToString(),
        });

        string fileName = string.Empty;
        string fileType = string.Empty;

        var memoryStream = new MemoryStream();

        await foreach (var msg in call.ResponseStream.ReadAllAsync())
        {
            if(msg.DataCase == FileDownloadResponse.DataOneofCase.Metadata){
                fileName = msg.Metadata.FileName;
                fileType = msg.Metadata.FileType.ToString();
            }
            else if(msg.DataCase == FileDownloadResponse.DataOneofCase.ChunkData){
                msg.ChunkData.WriteTo(memoryStream);
            }
        }

        memoryStream.Position = 0;
        var contentType = fileType.ToLower() switch
        {
            "pdf" => "application/pdf",
            "text" => "text/plain",
            "excel" => "application/vnd.ms-excel",
            _ => "application/octet-stream"
        };

        var fileBytes = memoryStream.ToArray();
        return new PageDownloadResponse
        {
            Content = new MemoryStream(fileBytes),
            FileName = fileName,
            ContentType = contentType
        };
    }

    public async Task<PageResponse> CreateAsync(Guid userId, string role, PageCreateRequest request)
    {
        var document = await _documentRepository.GetByIdAsync(request.DocumentId);
        if (document == null)
        {
            throw new InvalidOperationException($"Document with ID {request.DocumentId} not found.");
        }
        if (document.IsDeleted)
        {
            throw new InvalidOperationException($"Document with ID {request.DocumentId} is deleted.");
        }

        if (role != "Admin" && document.UserId != userId)
        {
            throw new UnauthorizedAccessException("You do not have permission to create a page for this document.");
        }

        if (request.PageNumber <= 0)
        {
            request.PageNumber = document.TotalPages + 1;
        }

        var pagesToShift = await _pageRepository.GetPagesGreaterThanOrEqualToAsync(
            request.DocumentId, request.PageNumber);

        // Update in reverse order to avoid unique constraint violations
        foreach (var p in pagesToShift.Reverse())
        {
            p.PageNumber++;
            await _pageRepository.UpdateAsync(p);
        }

        var file = await UploadFileAsync(request.Content);

        var page = new Page
        {
            Id = Guid.NewGuid(),
            DocumentId = request.DocumentId,
            PageNumber = request.PageNumber,
            FileId = Guid.Parse(file.Id),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var result = await _pageRepository.AddAsync(page);
        
        if (result == null)
        {
            throw new Exception("Internal server error: Failed to create page.");
        }

        document.TotalPages++;
        await _documentRepository.UpdateAsync(document);

        await _notificationService.NotifyPageCreatedAsync(document.Id, result.Id);

        return MapToResponse(result);
    }

    public async Task<PageResponse> UpdateAsync(Guid id, Guid userId, string role, PageUpdateRequest request)
    {
        var page = await _pageRepository.GetByIdAsync(id);

        if (page == null)
        {
            throw new KeyNotFoundException($"Page with ID {id} not found.");
        }

        var document = await _documentRepository.GetByIdAsync(page.DocumentId);
        if (document == null)
        {
            throw new InvalidOperationException($"Document with ID {page.DocumentId} not found.");
        }

        if (document.IsDeleted)
        {
            throw new InvalidOperationException($"Document with ID {page.DocumentId} is deleted.");
        }

        if (role != "Admin" && document.UserId != userId)
        {
            throw new UnauthorizedAccessException("You do not have permission to update this page.");
        }

        if (page.PageNumber != request.PageNumber)
        {
            var existingPage = await _pageRepository.GetByDocumentIdAndPageNumberAsync(
                page.DocumentId, request.PageNumber);

            if (existingPage != null)
            {
                var pagesToShift = await _pageRepository.GetPagesGreaterThanOrEqualToAsync(
                    page.DocumentId, request.PageNumber);

                // Update in reverse order to avoid unique constraint violations
                foreach (var p in pagesToShift.Where(p => p.Id != id).Reverse())
                {
                    p.PageNumber++;
                    await _pageRepository.UpdateAsync(p);
                }
            }
        }

        if (request.Content != null)
        {
            var file = await UploadFileAsync(request.Content);
            var oldFileId = page.FileId;
            
            page.FileId = Guid.Parse(file.Id);
            
            // Delete old file publish event rabbit mq here
            if(oldFileId != null)
            {
                await _publishEndpoint.Publish(new PageUpdatedEvent
                {
                    OldFileId = oldFileId
                });
            }
        }

        page.PageNumber = request.PageNumber;
        page.UpdatedAt = DateTime.UtcNow;

        await _pageRepository.UpdateAsync(page);

        await _notificationService.NotifyPageUpdatedAsync(page.DocumentId, page.Id);

        return MapToResponse(page);
    }

    public async Task DeleteAsync(Guid id, Guid userId, string role)
    {
        var page = await _pageRepository.GetByIdAsync(id);

        if (page == null)
        {
            throw new KeyNotFoundException($"Page with ID {id} not found.");
        }

        var document = await _documentRepository.GetByIdAsync(page.DocumentId);
        if (document == null)
        {
            throw new InvalidOperationException($"Document with ID {page.DocumentId} not found.");
        }
        if (document.IsDeleted)
        {
            throw new InvalidOperationException($"Document with ID {page.DocumentId} is deleted.");
        }

        if (role != "Admin" && document.UserId != userId)
        {
            throw new UnauthorizedAccessException("You do not have permission to delete this page.");
        }

        await _pageRepository.DeleteAsync(page);

        await _notificationService.NotifyPageDeletedAsync(document.Id, page.Id);

        // Delete file publish event rabbit mq here
        if(page.FileId != null)
        {
            await _publishEndpoint.Publish(new PageDeletedEvent
            {
                FileId = page.FileId
            });
        }
    }

    private static PageResponse MapToResponse(Page page)
    {
        return new PageResponse
        {
            Id = page.Id,
            DocumentId = page.DocumentId,
            DocumentTitle = page.Document?.Title ?? "Unknown",
            PageNumber = page.PageNumber,
            FileId = page.FileId ?? Guid.Empty,
            CreatedAt = page.CreatedAt,
            UpdatedAt = page.UpdatedAt
        };
    }

    private async Task<FileUploadResponse> UploadFileAsync(IFormFile content)
    {
        var year = DateTime.UtcNow.Year;
        var month = DateTime.UtcNow.Month;
        var day = DateTime.UtcNow.Day;
        var ext = Path.GetExtension(content.FileName);
        var fileName = $"{Guid.NewGuid()}{ext}";
        var filePath = $"{year}/{month}/{day}";

        using var call = _grpcClient.UploadFile();

        try
        {
            await call.RequestStream.WriteAsync(new UploadFileRequest
            {
                Metadata = new FileUploadMetadata
                {
                    FileName = fileName,
                    ContentType = content.ContentType,
                    FilePath = filePath
                }
            });

            const int chunkSize = 64 * 1024;
            var buffer = new byte[chunkSize];
            using var stream = content.OpenReadStream();
            int bytesRead;

            while ((bytesRead = await stream.ReadAsync(buffer)) > 0)
            {
                var chunk = UnsafeByteOperations.UnsafeWrap(buffer.AsMemory(0, bytesRead));

                await call.RequestStream.WriteAsync(new UploadFileRequest
                {
                    ChunkData = chunk
                });
            }

            await call.RequestStream.CompleteAsync();

            return await call.ResponseAsync;
        }
        catch (RpcException)
        {
            throw new Exception("File upload failed due to RPC error.");
        }
        catch (Exception ex)
        {
            throw new Exception("File upload failed.", ex);
        }
    }
}
