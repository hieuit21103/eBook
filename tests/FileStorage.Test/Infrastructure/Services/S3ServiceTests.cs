using Amazon.S3;
using Amazon.S3.Model;
using FileStorage.Application.Options;
using FileStorage.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace FileStorage.Test.Infrastructure.Services;

public class S3ServiceTests
{
    private readonly IAmazonS3 _s3Client;
    private readonly IOptions<S3Options> _s3Options;
    private readonly ILogger<S3Service> _logger;
    private readonly S3Service _service;

    public S3ServiceTests()
    {
        _s3Client = Substitute.For<IAmazonS3>();
        _s3Options = Substitute.For<IOptions<S3Options>>();
        _logger = Substitute.For<ILogger<S3Service>>();

        _s3Options.Value.Returns(new S3Options
        {
            BucketName = "test-bucket",
            PresignExpiration = "60"
        });

        _service = new S3Service(_s3Client, _s3Options, _logger);
    }

    [Fact]
    public async Task UploadFileAsync_ShouldUploadFile_WhenSuccessful()
    {
        // Arrange
        var fileName = "test.txt";
        var contentType = "text/plain";
        var content = "Hello World";
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));

        _s3Client.PutObjectAsync(Arg.Any<PutObjectRequest>(), Arg.Any<CancellationToken>())
            .Returns(new PutObjectResponse { HttpStatusCode = System.Net.HttpStatusCode.OK });

        // Act
        var result = await _service.UploadFileAsync(stream, fileName, contentType);

        // Assert
        result.Should().Be(fileName);
        await _s3Client.Received(1).PutObjectAsync(Arg.Is<PutObjectRequest>(r => 
            r.BucketName == "test-bucket" && 
            r.Key == fileName && 
            r.ContentType == contentType), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DownloadFileAsync_ShouldReturnStream_WhenFileExists()
    {
        // Arrange
        var s3Key = "test.txt";
        var content = "Hello World";
        var responseStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
        
        var response = new GetObjectResponse
        {
            ResponseStream = responseStream,
            HttpStatusCode = System.Net.HttpStatusCode.OK
        };

        _s3Client.GetObjectAsync(Arg.Any<GetObjectRequest>(), Arg.Any<CancellationToken>())
            .Returns(response);

        // Act
        var result = await _service.DownloadFileAsync(s3Key);

        // Assert
        result.Should().NotBeNull();
        using var reader = new StreamReader(result);
        var resultText = await reader.ReadToEndAsync();
        resultText.Should().Be(content);
    }

    [Fact]
    public async Task DeleteFileAsync_ShouldReturnTrue_WhenSuccessful()
    {
        // Arrange
        var s3Key = "test.txt";

        _s3Client.DeleteObjectAsync(Arg.Any<DeleteObjectRequest>(), Arg.Any<CancellationToken>())
            .Returns(new DeleteObjectResponse { HttpStatusCode = System.Net.HttpStatusCode.OK });

        // Act
        var result = await _service.DeleteFileAsync(s3Key);

        // Assert
        result.Should().BeTrue();
        await _s3Client.Received(1).DeleteObjectAsync(Arg.Is<DeleteObjectRequest>(r => 
            r.BucketName == "test-bucket" && 
            r.Key == s3Key), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task FileExistsAsync_ShouldReturnTrue_WhenFileExists()
    {
        // Arrange
        var s3Key = "test.txt";

        _s3Client.GetObjectMetadataAsync(Arg.Any<GetObjectMetadataRequest>(), Arg.Any<CancellationToken>())
            .Returns(new GetObjectMetadataResponse { HttpStatusCode = System.Net.HttpStatusCode.OK });

        // Act
        var result = await _service.FileExistsAsync(s3Key);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task FileExistsAsync_ShouldReturnFalse_WhenFileDoesNotExist()
    {
        // Arrange
        var s3Key = "non-existent.txt";

        _s3Client.GetObjectMetadataAsync(Arg.Any<GetObjectMetadataRequest>(), Arg.Any<CancellationToken>())
            .Throws(new AmazonS3Exception("Not Found") { StatusCode = System.Net.HttpStatusCode.NotFound });

        // Act
        var result = await _service.FileExistsAsync(s3Key);

        // Assert
        result.Should().BeFalse();
    }
}
