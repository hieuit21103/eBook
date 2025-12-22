using Grpc.Core;
using FileStorage.Application.Interfaces;
using FileStorage.Protos;
using Google.Protobuf.WellKnownTypes;
using Google.Protobuf;

namespace FileStorage.gRPC;

public class FileGrpcService : FileStorageService.FileStorageServiceBase
{
    private readonly IFileStorageService _service;
    private ILogger<FileGrpcService> _logger;

    public FileGrpcService(IFileStorageService service, ILogger<FileGrpcService> logger)
    {
        _service = service;
        _logger = logger;
    }

    public override async Task<FileUploadResponse> UploadFile(IAsyncStreamReader<UploadFileRequest> requestStream, ServerCallContext context)
    {
        var fileName = "";
        var contentType = "";
        var filePath = "";
        using var memoryStream = new MemoryStream();
        while (await requestStream.MoveNext())
        {
            var message = requestStream.Current;
            if(message.DataCase == UploadFileRequest.DataOneofCase.Metadata)
            {
                fileName = message.Metadata.FileName;
                contentType = message.Metadata.ContentType;
                filePath = message.Metadata.FilePath;
            }else if(message.DataCase == UploadFileRequest.DataOneofCase.ChunkData)
            {
                var chunk = requestStream.Current.ChunkData;
                if (chunk != null)
                {
                    chunk.WriteTo(memoryStream);
                }
            }
        }
        memoryStream.Position = 0;

        var result = await _service.UploadFileAsync(memoryStream, fileName, contentType, filePath);

        return new FileUploadResponse
        {
            Id = result.Id.ToString(),
            FileName = result.FileName,
            FilePath = result.FilePath,
            FileType = (FileType)result.FileType,
            CreatedAt = Timestamp.FromDateTime(result.CreatedAt.ToUniversalTime())
        };
    }

    public override async Task DownloadFile(FileRequest request, IServerStreamWriter<FileDownloadResponse> responseStream, ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out var guid))
             throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid GUID"));

        var result = await _service.DownloadFileAsync(guid);

        await responseStream.WriteAsync(new FileDownloadResponse
        {
            Metadata = new FileDownloadMetadata
            {
                FileName = result.FileName,
                FileType = (FileType)result.FileType
            }
        });

        using (result.FileStream)
        {
            var buffer = new byte[4096]; 
            int bytesRead;
            while ((bytesRead = await result.FileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await responseStream.WriteAsync(new FileDownloadResponse
                {
                    ChunkData = ByteString.CopyFrom(buffer, 0, bytesRead)
                });
            }
        }
    }

    public override async Task<FileMetadataResponse> GetFileMetadata(FileRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out var guid))
             throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid GUID"));

        var result = await _service.GetFileMetadataAsync(guid);
        if (result == null) throw new RpcException(new Status(StatusCode.NotFound, "File not found"));

        return new FileMetadataResponse
        {
            Id = result.Id.ToString(),
            FileName = result.FileName,
            FilePath = result.FilePath,
            FileType = (FileType)result.FileType,
            CreatedAt = Timestamp.FromDateTime(result.CreatedAt.ToUniversalTime()),
            UpdatedAt = Timestamp.FromDateTime(result.UpdatedAt.ToUniversalTime())
        };
    }

    public override async Task<DeleteResponse> DeleteFile(FileRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out var guid))
             throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid GUID"));

        var success = await _service.DeleteFileAsync(guid);
        return new DeleteResponse { Success = success };
    }

    public override async Task<UrlResponse> GetPresignedUrl(FileRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out var guid))
             throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid GUID"));

        var response = await _service.GetPresignedUrlAsync(guid);
        return new UrlResponse 
        { 
            FileType = (FileType)response.FileType, 
            Url = response.Url 
        };
    }
}


