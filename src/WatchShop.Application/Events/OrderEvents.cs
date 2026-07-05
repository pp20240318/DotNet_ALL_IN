namespace WatchShop.Application.Events;

public record OrderCreatedEvent(long OrderId, string OrderNo, DateTime CreatedAt);

public record OrderCancelledEvent(long OrderId, string OrderNo, string Reason);
