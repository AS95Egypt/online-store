namespace OnlineStore.Application.Orders;

/// <summary>One order row in the administrator's order list.</summary>
public sealed record AdminOrderListItemDto(
    int Id,
    string OrderNumber,
    DateTimeOffset CreatedAt,
    string CustomerName,
    int ItemsCount,
    decimal Total,
    string Status,
    string PaymentStatus);

/// <summary>Order totals grouped by lifecycle status.</summary>
public sealed record OrderStatusCountsDto(
    int All,
    int Pending,
    int Confirmed,
    int Processing,
    int Shipped,
    int Delivered,
    int Cancelled);

/// <summary>A page of orders and counts for the status filters.</summary>
public sealed record AdminOrderListResultDto(
    IReadOnlyList<AdminOrderListItemDto> Items,
    int TotalCount,
    OrderStatusCountsDto Counts);

/// <summary>A status-history entry for an administrator.</summary>
public sealed record AdminOrderStatusHistoryDto(
    string Status,
    DateTimeOffset ChangedAt,
    string? ChangedBy);

/// <summary>Actions currently permitted for an order.</summary>
public sealed record AdminOrderAvailableActionsDto(
    string? NextStatus,
    bool CanCancel,
    bool CanMarkPaid);

/// <summary>The full order snapshot and available operations for an administrator.</summary>
public sealed record AdminOrderDetailsDto(
    int Id,
    string OrderNumber,
    DateTimeOffset CreatedAt,
    string Status,
    string PaymentMethod,
    string PaymentStatus,
    DateTimeOffset? PaidAt,
    string CustomerName,
    string CustomerPhone,
    decimal Subtotal,
    decimal ShippingFee,
    decimal Total,
    string ShippingFullName,
    string ShippingPhone,
    string ShippingGovernorateName,
    string ShippingCity,
    string ShippingDistrict,
    string ShippingStreet,
    string ShippingBuildingNumber,
    string? ShippingApartment,
    string? ShippingInstructions,
    IReadOnlyList<AdminOrderItemDto> Items,
    IReadOnlyList<AdminOrderStatusHistoryDto> StatusHistory,
    AdminOrderAvailableActionsDto AvailableActions);

/// <summary>An item snapshot stored when the order was placed.</summary>
public sealed record AdminOrderItemDto(
    string ProductNameSnapshot,
    string SkuSnapshot,
    decimal UnitPriceSnapshot,
    int Quantity,
    decimal LineTotal);

/// <summary>Request to advance an order to its next status.</summary>
public sealed record UpdateOrderStatusRequest
{
    public required string Status { get; init; }
}

/// <summary>A requested admin order operation is not valid for the order's current state.</summary>
public sealed class AdminOrderConflictException : Exception
{
    public string Code { get; }

    public AdminOrderConflictException(string code, string message)
        : base(message)
    {
        Code = code;
    }
}

/// <summary>The requested order does not exist.</summary>
public sealed class AdminOrderNotFoundException : Exception
{
    public AdminOrderNotFoundException(int orderId)
        : base($"Order {orderId} was not found.")
    {
    }
}
