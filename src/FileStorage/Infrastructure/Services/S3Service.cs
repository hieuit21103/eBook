using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using FileStorage.Application.Interfaces;
using FileStorage.Application.Options;
using Microsoft.Extensions.Options;

namespace FileStorage.Infrastructure.Services;

public class S3Service : IS3Service
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;
    private readonly string _presignExpiration;
    private readonly S3Options _s3Options;
    private readonly ILogger<S3Service> _logger;

    public S3Service(
        IAmazonS3 s3Client,
        IOptions<S3Options> s3Options,
        ILogger<S3Service> logger)
    {
        _s3Client = s3Client;
        _s3Options = s3Options.Value;
        _bucketName = _s3Options.BucketName;
        _presignExpiration = _s3Options.PresignExpiration;
        _logger = logger;
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
    {
        try
        {
            var uploadRequest = new PutObjectRequest
            {
                InputStream = fileStream,
                Key = fileName,
                BucketName = _bucketName,
                ContentType = contentType,
                DisablePayloadSigning = true,
                CannedACL = S3CannedACL.Private
            };

            await _s3Client.PutObjectAsync(uploadRequest);
            _logger.LogInformation("File uploaded successfully: {FileName}", fileName);

            return fileName;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file to S3: {FileName}", fileName);
            throw;
        }
    }

    public async Task<Stream> DownloadFileAsync(string s3Key)
    {
        try
        {
            var request = new GetObjectRequest
            {
                BucketName = _bucketName,
                Key = s3Key
            };

            var response = await _s3Client.GetObjectAsync(request);
            var memoryStream = new MemoryStream();
            await response.ResponseStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            return memoryStream;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file from S3: {S3Key}", s3Key);
            throw;
        }
    }

    public async Task<bool> DeleteFileAsync(string s3Key)
    {
        try
        {
            var request = new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = s3Key
            };

            await _s3Client.DeleteObjectAsync(request);
            _logger.LogInformation("File deleted successfully: {S3Key}", s3Key);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file from S3: {S3Key}", s3Key);
            return false;
        }
    }

    public async Task<bool> FileExistsAsync(string s3Key)
    {
        try
        {
            var request = new GetObjectMetadataRequest
            {
                BucketName = _bucketName,
                Key = s3Key
            };

            await _s3Client.GetObjectMetadataAsync(request);
            return true;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking file existence in S3: {S3Key}", s3Key);
            throw;
        }
    }

    public async Task<string> GetPresignedUrlAsync(string s3Key)
    {
        try
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _bucketName,
                Key = s3Key,
                Expires = DateTime.UtcNow.AddMinutes(int.Parse(_presignExpiration)),
                Verb = HttpVerb.GET
            };

            var url = await _s3Client.GetPreSignedURLAsync(request);
            return url;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating presigned URL: {S3Key}", s3Key);
            throw;
        }
    }
}
