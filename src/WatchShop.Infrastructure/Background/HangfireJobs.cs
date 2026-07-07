using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WatchShop.Application.Abstractions;
using WatchShop.Application.Contracts.Persistence;
using WatchShop.Application.Events;
using WatchShop.Application.Options;
using WatchShop.Domain.Entities;
using WatchShop.Domain.Enums;
using SqlSugar;

namespace WatchShop.Infrastructure.Background;

public class OrderTimeoutJob
{
    private readonly IServiceProvider _serviceProvider;
    private readonly OrderOptions _options;
    private readonly ILogger<OrderTimeoutJob> _logger;

    public OrderTimeoutJob(
        IServiceProvider serviceProvider,
        IOptions<OrderOptions> options,
        ILogger<OrderTimeoutJob> logger)
    {
        _serviceProvider = serviceProvider;
        _options = options.Value;
        _logger = logger;
    }

    public async Task RunAsync()
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
            .ToListAsync();

        foreach (var order in expiredOrders)
        {
            await unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var items = await db.Queryable<OrderItem>()
                    .Where(x => x.OrderId == order.Id && !x.IsDeleted)
                    .ToListAsync();

                foreach (var item in items)
                {
                    var sku = await unitOfWork.Repository<ProductSku>().GetByIdAsync(item.SkuId);
                    if (sku is not null)
                    {
                        sku.Stock += item.Quantity;
                        await unitOfWork.Repository<ProductSku>().UpdateAsync(sku);
                    }
                }

                order.Status = OrderStatus.Cancelled;
                order.CancelledAt = DateTime.UtcNow;
                order.UpdatedAt = DateTime.UtcNow;
                await unitOfWork.Repository<ShopOrder>().UpdateAsync(order);
                await eventPublisher.PublishAsync(
                    new OrderCancelledEvent(order.Id, order.OrderNo, "支付超时自动取消"));
            });

            _logger.LogInformation("Order {OrderNo} auto cancelled by Hangfire.", order.OrderNo);
        }
    }
}

public class SearchIndexSyncJob
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ElasticsearchOptions _options;
    private readonly ILogger<SearchIndexSyncJob> _logger;

    public SearchIndexSyncJob(
        IServiceProvider serviceProvider,
        IOptions<ElasticsearchOptions> options,
        ILogger<SearchIndexSyncJob> logger)
    {
        _serviceProvider = serviceProvider;
        _options = options.Value;
        _logger = logger;
    }

    public async Task RunAsync()
    {
        if (!_options.Enabled)
        {
            return;
        }

        using var scope = _serviceProvider.CreateScope();
        var es = scope.ServiceProvider.GetRequiredService<Search.ElasticsearchSearchService>();
        await es.SyncIndexAsync();
        _logger.LogInformation("Elasticsearch index synced.");
    }
}
