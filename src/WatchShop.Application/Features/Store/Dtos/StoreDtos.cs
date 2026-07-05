using WatchShop.Domain.Enums;

namespace WatchShop.Application.Features.Store.Dtos;

public class CustomerRegisterRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? Nickname { get; set; }
    public string? Phone { get; set; }
}

public class CustomerLoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class CustomerLoginResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public CustomerProfileResponse Profile { get; set; } = new();
}

public class CustomerProfileResponse
{
    public long Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? Nickname { get; set; }
    public string? Phone { get; set; }
}

public class CartItemResponse
{
    public long SkuId { get; set; }
    public string SkuCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public decimal Subtotal => Price * Quantity;
}

public class CartAddRequest
{
    public long SkuId { get; set; }
    public int Quantity { get; set; } = 1;
}

public class CartUpdateRequest
{
    public int Quantity { get; set; }
}

public class CartCheckoutRequest
{
    public string ReceiverName { get; set; } = string.Empty;
    public string ReceiverPhone { get; set; } = string.Empty;
    public string ReceiverAddress { get; set; } = string.Empty;
}

public class StoreOrderSummaryResponse
{
    public long Id { get; set; }
    public string OrderNo { get; set; } = string.Empty;
    public OrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class StoreOrderDetailResponse : StoreOrderSummaryResponse
{
    public string? ReceiverName { get; set; }
    public string? ReceiverPhone { get; set; }
    public string? ReceiverAddress { get; set; }
    public DateTime? PaidAt { get; set; }
    public List<StoreOrderItemResponse> Items { get; set; } = [];
}

public class StoreOrderItemResponse
{
    public string ProductName { get; set; } = string.Empty;
    public string SkuCode { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}

public class DashboardStatsResponse
{
    public int ProductCount { get; set; }
    public int OrderCount { get; set; }
    public int CustomerCount { get; set; }
    public int PendingPaymentCount { get; set; }
    public decimal TodayOrderAmount { get; set; }
}

public class OperationLogResponse
{
    public long Id { get; set; }
    public string AdminName { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? RequestPath { get; set; }
    public string? RequestMethod { get; set; }
    public bool IsSuccess { get; set; }
    public DateTime CreatedAt { get; set; }
}
