using System.ComponentModel.DataAnnotations;
using OnlineStore.Application.Customers;

namespace OnlineStore.Application.Orders;

public record OrderSummaryDto(
    string OrderNumber,
    DateTime CreatedAt,
    int ItemsCount,
    decimal Total,
    string Status);

public record OrderItemDto(
    string ProductName,
    string Sku,
    decimal UnitPrice,
    int Quantity,
    decimal LineTotal);

public record OrderStatusHistoryDto(string Status, DateTime ChangedAt);

public record OrderDetailsDto(
    string OrderNumber,
    DateTime CreatedAt,
    string Status,
    string PaymentMethod,
    string PaymentStatus,
    decimal Subtotal,
    decimal ShippingFee,
    decimal Total,
    IReadOnlyList<OrderItemDto> Items,
    CustomerAddressDto ShippingAddress,
    IReadOnlyList<OrderStatusHistoryDto> StatusHistory);

public sealed class CreateOrderRequest
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string ClientRequestId { get; set; } = string.Empty;

    [Required]
    public List<OrderPriceExpectation> ExpectedItems { get; set; } = new();

    [Range(typeof(decimal), "0", "9999999999999999")]
    public decimal ExpectedShippingFee { get; set; }
}

public record OrderPriceExpectation(int ProductId, int Quantity, decimal UnitPrice);

public record OrderItemError(int ProductId, string Reason);

public sealed class OrderValidationException : Exception
{
    public string Code { get; }
    public IReadOnlyList<OrderItemError> Errors { get; }

    public OrderValidationException(
        string code,
        string message,
        IReadOnlyList<OrderItemError>? errors = null)
        : base(message)
    {
        Code = code;
        Errors = errors ?? Array.Empty<OrderItemError>();
    }
}

public sealed class OrderNotCancellableException : Exception
{
    public string Code { get; }

    public OrderNotCancellableException(string code, string message)
        : base(message)
    {
        Code = code;
    }
}
