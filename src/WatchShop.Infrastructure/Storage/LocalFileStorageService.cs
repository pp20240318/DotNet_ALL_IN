using Microsoft.Extensions.Options;
using WatchShop.Application.Abstractions;
using WatchShop.Application.Exceptions;
using WatchShop.Application.Options;

namespace WatchShop.Infrastructure.Storage;

public class LocalFileStorageService : IFileStorageService
{
    private readonly FileStorageOptions _options;

    public LocalFileStorageService(IOptions<FileStorageOptions> options)
    {
        _options = options.Value;
    }

    public async Task<string> SaveAsync(Stream content, string fileName, string folder, CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (!_options.AllowedExtensions.Contains(extension))
        {
            throw new BusinessException($"不支持的文件类型: {extension}");
        }

        if (content.CanSeek && content.Length > _options.MaxFileSizeBytes)
        {
            throw new BusinessException("文件大小超出限制");
        }

        var targetDir = Path.Combine(_options.RootPath, folder);
        Directory.CreateDirectory(targetDir);

        var storedName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(targetDir, storedName);

        await using var fileStream = File.Create(fullPath);
        await content.CopyToAsync(fileStream, cancellationToken);

        return $"/uploads/{folder}/{storedName}".Replace('\\', '/');
    }

    public Task DeleteAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return Task.CompletedTask;
        }

        var normalized = relativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.Combine("wwwroot", normalized);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }
}
