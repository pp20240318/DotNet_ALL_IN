using MediatR;
using WatchShop.Application.Abstractions;
using WatchShop.Application.Contracts.Persistence;
using WatchShop.Application.Exceptions;
using WatchShop.Application.Features.Catalog;
using WatchShop.Domain.Entities;

namespace WatchShop.Application.Features.Products.Commands;

public record UploadProductCoverCommand(long ProductId, Stream Content, string FileName) : IRequest<string>;

public class UploadProductCoverCommandHandler : IRequestHandler<UploadProductCoverCommand, string>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorage;
    private readonly ICatalogCacheInvalidator _catalogCacheInvalidator;

    public UploadProductCoverCommandHandler(
        IUnitOfWork unitOfWork,
        IFileStorageService fileStorage,
        ICatalogCacheInvalidator catalogCacheInvalidator)
    {
        _unitOfWork = unitOfWork;
        _fileStorage = fileStorage;
        _catalogCacheInvalidator = catalogCacheInvalidator;
    }

    public async Task<string> Handle(UploadProductCoverCommand request, CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Repository<Product>().GetByIdAsync(request.ProductId, cancellationToken)
            ?? throw new BusinessException("商品不存在");

        if (!string.IsNullOrWhiteSpace(product.CoverImage))
        {
            await _fileStorage.DeleteAsync(product.CoverImage, cancellationToken);
        }

        var url = await _fileStorage.SaveAsync(request.Content, request.FileName, "products", cancellationToken);
        product.CoverImage = url;
        await _unitOfWork.Repository<Product>().UpdateAsync(product, cancellationToken);
        await _catalogCacheInvalidator.InvalidateProductAsync(product.Id, cancellationToken);
        return url;
    }
}
