using WatchShop.Application.Abstractions;
using WatchShop.Application.Contracts.Persistence;
using WatchShop.Application.Events;
using WatchShop.Application.Exceptions;
using WatchShop.Application.Features.Store;
using WatchShop.Application.Features.Store.Dtos;
using WatchShop.Domain.Entities;
using WatchShop.Domain.Enums;

namespace WatchShop.Infrastructure.Services;

public class CartService : ICartService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventPublisher _eventPublisher;

    public CartService(IUnitOfWork unitOfWork, IEventPublisher eventPublisher)
    {
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
    }

    public async Task<List<CartItemResponse>> GetCartAsync(long customerId, CancellationToken cancellationToken = default)
    {
        var items = await _unitOfWork.Repository<CartItem>()
            .GetListAsync(x => x.CustomerId == customerId, cancellationToken);

        var result = new List<CartItemResponse>();
        foreach (var item in items)
        {
            result.Add(await MapCartItem(item, cancellationToken));
        }

        return result;
    }

    public async Task AddItemAsync(long customerId, CartAddRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Quantity <= 0)
        {
            throw new BusinessException("数量必须大于 0");
        }

        var sku = await _unitOfWork.Repository<ProductSku>().GetByIdAsync(request.SkuId, cancellationToken)
            ?? throw new BusinessException("SKU 不存在");

        if (!sku.IsEnabled)
        {
            throw new BusinessException("SKU 已下架");
        }

        var existing = (await _unitOfWork.Repository<CartItem>()
            .GetListAsync(x => x.CustomerId == customerId && x.SkuId == request.SkuId, cancellationToken))
            .FirstOrDefault();

        if (existing is null)
        {
            await _unitOfWork.Repository<CartItem>().InsertAsync(new CartItem
            {
                CustomerId = customerId,
                SkuId = request.SkuId,
                Quantity = request.Quantity
            }, cancellationToken);
        }
        else
        {
            existing.Quantity += request.Quantity;
            await _unitOfWork.Repository<CartItem>().UpdateAsync(existing, cancellationToken);
        }
    }

    public async Task UpdateQuantityAsync(long customerId, long skuId, int quantity, CancellationToken cancellationToken = default)
    {
        var item = (await _unitOfWork.Repository<CartItem>()
            .GetListAsync(x => x.CustomerId == customerId && x.SkuId == skuId, cancellationToken))
            .FirstOrDefault()
            ?? throw new BusinessException("购物车项不存在");

        if (quantity <= 0)
        {
            await _unitOfWork.Repository<CartItem>().SoftDeleteAsync(item.Id, cancellationToken);
            return;
        }

        item.Quantity = quantity;
        await _unitOfWork.Repository<CartItem>().UpdateAsync(item, cancellationToken);
    }

    public async Task RemoveItemAsync(long customerId, long skuId, CancellationToken cancellationToken = default)
    {
        var item = (await _unitOfWork.Repository<CartItem>()
            .GetListAsync(x => x.CustomerId == customerId && x.SkuId == skuId, cancellationToken))
            .FirstOrDefault()
            ?? throw new BusinessException("购物车项不存在");

        await _unitOfWork.Repository<CartItem>().SoftDeleteAsync(item.Id, cancellationToken);
    }

    public async Task<long> CheckoutAsync(long customerId, CartCheckoutRequest request, CancellationToken cancellationToken = default)
    {
        var cartItems = await _unitOfWork.Repository<CartItem>()
            .GetListAsync(x => x.CustomerId == customerId, cancellationToken);

        if (cartItems.Count == 0)
        {
            throw new BusinessException("购物车为空");
        }

        long orderId = 0;
        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            decimal total = 0;
            var orderItems = new List<OrderItem>();

            foreach (var cart in cartItems)
            {
                var sku = await _unitOfWork.Repository<ProductSku>().GetByIdAsync(cart.SkuId, cancellationToken)
                    ?? throw new BusinessException($"SKU {cart.SkuId} 不存在");

                if (!sku.IsEnabled || sku.Stock < cart.Quantity)
                {
                    throw new BusinessException($"SKU {sku.SkuCode} 库存不足");
                }

                var product = await _unitOfWork.Repository<Product>().GetByIdAsync(sku.ProductId, cancellationToken)
                    ?? throw new BusinessException("商品不存在");

                if (product.Status != ProductStatus.OnSale)
                {
                    throw new BusinessException($"商品 {product.Name} 未上架");
                }

                sku.Stock -= cart.Quantity;
                await _unitOfWork.Repository<ProductSku>().UpdateAsync(sku, cancellationToken);
                total += sku.Price * cart.Quantity;

                orderItems.Add(new OrderItem
                {
                    ProductId = product.Id,
                    SkuId = sku.Id,
                    ProductName = product.Name,
                    SkuCode = sku.SkuCode,
                    Price = sku.Price,
                    Quantity = cart.Quantity
                });
            }

            var order = new ShopOrder
            {
                OrderNo = $"WS{DateTime.UtcNow:yyyyMMddHHmmss}{Random.Shared.Next(1000, 9999)}",
                Status = OrderStatus.PendingPayment,
                TotalAmount = total,
                CustomerId = customerId,
                ReceiverName = request.ReceiverName,
                ReceiverPhone = request.ReceiverPhone,
                ReceiverAddress = request.ReceiverAddress
            };
            orderId = await _unitOfWork.Repository<ShopOrder>().InsertAsync(order, cancellationToken);

            foreach (var item in orderItems)
            {
                item.OrderId = orderId;
                await _unitOfWork.Repository<OrderItem>().InsertAsync(item, cancellationToken);
            }

            foreach (var cart in cartItems)
            {
                await _unitOfWork.Repository<CartItem>().SoftDeleteAsync(cart.Id, cancellationToken);
            }

            await _eventPublisher.PublishAsync(
                new OrderCreatedEvent(orderId, order.OrderNo, order.CreatedAt),
                cancellationToken);
        }, cancellationToken);

        return orderId;
    }

    private async Task<CartItemResponse> MapCartItem(CartItem item, CancellationToken cancellationToken)
    {
        var sku = await _unitOfWork.Repository<ProductSku>().GetByIdAsync(item.SkuId, cancellationToken)
            ?? throw new BusinessException("SKU 不存在");
        var product = await _unitOfWork.Repository<Product>().GetByIdAsync(sku.ProductId, cancellationToken)
            ?? throw new BusinessException("商品不存在");

        return new CartItemResponse
        {
            SkuId = sku.Id,
            SkuCode = sku.SkuCode,
            ProductName = product.Name,
            Price = sku.Price,
            Quantity = item.Quantity
        };
    }
}
