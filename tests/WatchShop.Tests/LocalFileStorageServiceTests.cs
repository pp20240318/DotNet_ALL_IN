using WatchShop.Application.Abstractions;
using WatchShop.Application.Options;
using WatchShop.Infrastructure.Storage;

namespace WatchShop.Tests;

public class LocalFileStorageServiceTests
{
    [Fact]
    public async Task SaveAsync_Should_Return_Public_Url()
    {
        var options = Microsoft.Extensions.Options.Options.Create(new FileStorageOptions
        {
            RootPath = Path.Combine(Path.GetTempPath(), "watchshop-test-uploads"),
            MaxFileSizeBytes = 1024 * 1024,
            AllowedExtensions = [".png"]
        });
        var service = new LocalFileStorageService(options);
        await using var stream = new MemoryStream([0x89, 0x50, 0x4E, 0x47]);

        var url = await service.SaveAsync(stream, "test.png", "products");

        Assert.StartsWith("/uploads/products/", url);
    }
}
