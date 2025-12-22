namespace FileStorage.Application.Options;

public class S3Options
{
    public string BucketName { get; set; } = string.Empty;
    public string PresignExpiration { get; set; } = string.Empty;
}