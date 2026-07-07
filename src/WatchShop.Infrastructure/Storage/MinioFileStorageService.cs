using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using WatchShop.Application.Abstractions;
using WatchShop.Application.Exceptions;
using WatchShop.Application.Options;

namespace WatchShop.Infrastructure.Storage;

public class MinioFileStorageService : IFileStorageService
{
    private readonly FileStorageOptions _fileOptions;
    private readonly MinioOptions _minioOptions;
    private readonly IMinioClient _client;

    public MinioFileStorageService(IOptions<FileStorageOptions> fileOptions, IOptions<MinioOptions> minioOptions)
    {
        _fileOptions = fileOptions.Value;
        _minioOptions = minioOptions.Value;
        _client = new MinioClient()
            .WithEndpoint(_minioOptions.Endpoint)
            .WithCredentials(_minioOptions.AccessKey, _minioOptions.SecretKey)
            .WithSSL(_minioOptions.UseSsl)
            .Build();
    }

    public async Task<string> SaveAsync(Stream content, string fileName, string folder, CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (!_fileOptions.AllowedExtensions.Contains(extension))
        {
            throw new BusinessException($"不支持的文件类型: {extension}");
        }

        await EnsureBucketAsync(cancellationToken);

        var objectName = $"{folder}/{Guid.NewGuid():N}{extension}";
        var putArgs = new PutObjectArgs()
            .WithBucket(_minioOptions.Bucket)
            .WithObject(objectName)
            .WithStreamData(content)
            .WithObjectSize(content.CanSeek ? content.Length : -1)
            .WithContentType(GetContentType(extension));

        await _client.PutObjectAsync(putArgs, cancellationToken);
        return $"{_minioOptions.PublicBaseUrl.TrimEnd('/')}/{_minioOptions.Bucket}/{objectName}";
    }

    public async Task DeleteAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return;
        }

        var marker = $"/{_minioOptions.Bucket}/";
        var index = relativePath.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (index < 0)
        {
            return;
        }

        var objectName = relativePath[(index + marker.Length)..];
        await _client.RemoveObjectAsync(new RemoveObjectArgs()
            .WithBucket(_minioOptions.Bucket)
            .WithObject(objectName), cancellationToken);
    }

    private async Task EnsureBucketAsync(CancellationToken cancellationToken)
    {
        var exists = await _client.BucketExistsAsync(
            new BucketExistsArgs().WithBucket(_minioOptions.Bucket), cancellationToken);
        if (!exists)
        {
            await _client.MakeBucketAsync(
                new MakeBucketArgs().WithBucket(_minioOptions.Bucket), cancellationToken);
        }
    }

    private static string GetContentType(string extension) => extension switch
    {
        ".jpg" or ".jpeg" => "image/jpeg",
        ".png" => "image/png",
        ".webp" => "image/webp",
        ".gif" => "image/gif",
        _ => "application/octet-stream"
    };
}
