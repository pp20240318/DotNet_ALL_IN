using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WatchShop.Application.Abstractions;
using WatchShop.Application.Contracts.Persistence;
using WatchShop.Application.Events;
using WatchShop.Application.Options;
using SqlSugar;
using WatchShop.Domain.Entities;
using WatchShop.Domain.Enums;

namespace WatchShop.Infrastructure.Background;

public class OrderTimeoutBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly OrderOptions _options;
    private readonly ILogger<OrderTimeoutBackgroundService> _logger;

    public OrderTimeoutBackgroundService(
        IServiceProvider serviceProvider,
        IOptions<OrderOptions> options,
        ILogger<OrderTimeoutBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ISqlSugarClient>();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var eventPublisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();
                var deadline = DateTime.UtcNow.AddMinutes(-_options.PaymentTimeoutMinutes);

                var expiredOrders = await db.Queryable<ShopOrder>()
                    .Where(x => !x.IsDeleted &&
                                x.Status == OrderStatus.PendingPayment &&
                                x.CreatedAt <= deadline)
                    .ToListAsync(stoppingToken);

                foreach (var order in expiredOrders)
                {
                    await unitOfWork.ExecuteInTransactionAsync(async () =>
                    {
                        var items = await db.Queryable<OrderItem>()
                            .Where(x => x.OrderId == order.Id && !x.IsDeleted)
                            .ToListAsync();

                        foreach (var item in items)
                        {
                            var sku = await unitOfWork.Repository<ProductSku>().GetByIdAsync(item.SkuId, stoppingToken);
                            if (sku is not null)
                            {
                                sku.Stock += item.Quantity;
                                await unitOfWork.Repository<ProductSku>().UpdateAsync(sku, stoppingToken);
                            }
                        }

                        order.Status = OrderStatus.Cancelled;
                        order.CancelledAt = DateTime.UtcNow;
                        order.UpdatedAt = DateTime.UtcNow;
                        await unitOfWork.Repository<ShopOrder>().UpdateAsync(order, stoppingToken);
                        await eventPublisher.PublishAsync(
                            new OrderCancelledEvent(order.Id, order.OrderNo, "支付超时自动取消"),
                            stoppingToken);
                    }, stoppingToken);

                    _logger.LogInformation("Order {OrderNo} auto cancelled due to payment timeout.", order.OrderNo);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Order timeout background task failed.");
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
