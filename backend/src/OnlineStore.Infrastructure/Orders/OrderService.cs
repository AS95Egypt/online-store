using System.Data;
using Microsoft.EntityFrameworkCore;
using OnlineStore.Application.Cart;
using OnlineStore.Application.Customers;
using OnlineStore.Application.Orders;
using OnlineStore.Domain.Entities;
using OnlineStore.Infrastructure.Carts;
using OnlineStore.Infrastructure.Persistence;

namespace OnlineStore.Infrastructure.Orders;

public sealed class OrderService : IOrderService
{
    private readonly AppDbContext _dbContext;

    public OrderService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<OrderDetailsDto> CreateOrderAsync(
        string userId,
        CreateOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.ClientRequestId))
        {
            throw new OrderValidationException(
                "invalid_request_id",
                "A client request ID is required.");
        }
        var clientRequestId = request.ClientRequestId.Trim();

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        var existingOrder = await _dbContext.Orders
            .AsNoTracking()
            .Where(order => order.UserId == userId && order.ClientRequestId == clientRequestId)
            .Select(order => order.OrderNumber)
            .SingleOrDefaultAsync(cancellationToken);
        if (existingOrder is not null)
        {
            await transaction.CommitAsync(cancellationToken);
            return await GetRequiredOrderDetailsAsync(userId, existingOrder, cancellationToken);
        }

        var user = await _dbContext.Users
            .AsNoTracking()
            .Include(candidate => candidate.ShippingGovernorate)
            .SingleOrDefaultAsync(candidate => candidate.Id == userId, cancellationToken);
        if (user is null)
        {
            throw new OrderValidationException("customer_not_found", "Customer account was not found.");
        }

        if (!HasCompleteShippingAddress(user))
        {
            throw new OrderValidationException(
                "address_required",
                "Complete and save your shipping address before placing the order.");
        }

        var governorate = user.ShippingGovernorate;
        if (governorate is null)
        {
            throw new OrderValidationException(
                "invalid_governorate",
                "Select a valid governorate in your shipping address.");
        }

        var cart = await _dbContext.Carts
            .FromSqlInterpolated(
                $"SELECT [Id], [UserId] FROM [Carts] WITH (UPDLOCK, HOLDLOCK) WHERE [UserId] = {userId}")
            .AsNoTracking()
            .SingleOrDefaultAsync(cancellationToken);
        if (cart is null)
        {
            throw new OrderValidationException("cart_empty", "Your cart is empty.");
        }

        var cartItems = await _dbContext.CartItems
            .AsNoTracking()
            .Where(item => item.CartId == cart.Id)
            .OrderBy(item => item.ProductId)
            .ToListAsync(cancellationToken);
        if (cartItems.Count == 0)
        {
            throw new OrderValidationException("cart_empty", "Your cart is empty.");
        }

        if (request.ExpectedItems is null ||
            request.ExpectedItems.Count > 100 ||
            request.ExpectedItems.Select(item => item.ProductId).Distinct().Count() != request.ExpectedItems.Count ||
            request.ExpectedItems.Any(item => item.ProductId <= 0 || item.Quantity < 1 || item.UnitPrice < 0))
        {
            throw new OrderValidationException(
                "invalid_order_request",
                "The checkout request contains invalid item expectations.");
        }

        var expectedItems = request.ExpectedItems.ToDictionary(item => item.ProductId);
        var cartProductIds = cartItems.Select(item => item.ProductId).ToHashSet();
        var priceErrors = cartItems
            .Where(item => !expectedItems.TryGetValue(item.ProductId, out var expected) ||
                expected.Quantity != item.Quantity)
            .Select(item => new OrderItemError(
                item.ProductId,
                "The cart changed. Review the updated order summary before placing your order."))
            .ToList();
        priceErrors.AddRange(
            expectedItems.Keys
                .Where(productId => !cartProductIds.Contains(productId))
                .Select(productId => new OrderItemError(
                    productId,
                    "The cart changed. Review the updated order summary before placing your order.")));
        if (priceErrors.Count > 0)
        {
            throw new OrderValidationException(
                "checkout_changed",
                "The cart changed. Review the updated order summary before placing your order.",
                priceErrors);
        }

        if (request.ExpectedShippingFee != governorate.ShippingFee)
        {
            throw new OrderValidationException(
                "checkout_changed",
                "The shipping fee changed. Review the updated totals before placing your order.");
        }

        foreach (var item in cartItems)
        {
            var expected = expectedItems[item.ProductId];
            var affectedRows = await _dbContext.Products
                .Where(product =>
                    product.Id == item.ProductId &&
                    product.IsActive &&
                    product.StockStatus == ProductStockStatuses.InStock &&
                    product.Qty >= item.Quantity &&
                    product.Price == expected.UnitPrice)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(product => product.Qty, product => product.Qty - item.Quantity)
                        .SetProperty(
                            product => product.StockStatus,
                            product => product.Qty - item.Quantity == 0
                                ? ProductStockStatuses.OutOfStock
                                : ProductStockStatuses.InStock),
                    cancellationToken);
            if (affectedRows != 1)
            {
                var product = await _dbContext.Products
                    .AsNoTracking()
                    .SingleOrDefaultAsync(candidate => candidate.Id == item.ProductId, cancellationToken);
                var reason = product is null
                    ? "This product is no longer available."
                    : CartValidation.GetProductError(product, item.Quantity) ??
                      "The product price changed. Review the updated order summary before placing your order.";
                throw new OrderValidationException(
                    "checkout_changed",
                    "One or more cart items changed. Review the updated order summary before placing your order.",
                    [new OrderItemError(item.ProductId, reason)]);
            }
        }

        var products = await _dbContext.Products
            .AsNoTracking()
            .Where(product => cartProductIds.Contains(product.Id))
            .ToDictionaryAsync(product => product.Id, cancellationToken);
        foreach (var item in cartItems)
        {
            item.Product = products[item.ProductId];
        }

        var subtotal = cartItems.Sum(item => item.Product.Price * item.Quantity);
        var now = DateTime.UtcNow;
        var yearStart = new DateTime(now.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var nextYearStart = yearStart.AddYears(1);
        var ordersThisYear = await _dbContext.Orders
            .FromSqlInterpolated(
                $"SELECT * FROM [Orders] WITH (UPDLOCK, HOLDLOCK) WHERE [CreatedAt] >= {yearStart} AND [CreatedAt] < {nextYearStart}")
            .AsNoTracking()
            .CountAsync(cancellationToken);
        var order = new Order
        {
            OrderNumber = $"ORD-{now:yyyy}-{ordersThisYear + 1:000000}",
            UserId = userId,
            ClientRequestId = clientRequestId,
            CreatedAt = now,
            Status = OrderStatuses.Pending,
            PaymentMethod = "COD",
            PaymentStatus = "Pending",
            Subtotal = subtotal,
            ShippingFee = governorate.ShippingFee,
            Total = subtotal + governorate.ShippingFee,
            ShippingFullName = user.ShippingFullName!.Trim(),
            ShippingPhone = user.ShippingPhone!.Trim(),
            ShippingGovernorateId = governorate.Id,
            ShippingGovernorateName = governorate.Name,
            ShippingCity = user.ShippingCity!.Trim(),
            ShippingDistrict = user.ShippingDistrict!.Trim(),
            ShippingStreet = user.ShippingStreet!.Trim(),
            ShippingBuildingNumber = user.ShippingBuildingNumber!.Trim(),
            ShippingApartment = user.ShippingApartment,
            ShippingInstructions = user.ShippingInstructions,
            Items = cartItems.Select(item => new OrderItem
            {
                ProductId = item.ProductId,
                ProductNameSnapshot = item.Product.ItemNameEn,
                SkuSnapshot = item.Product.Sku,
                UnitPriceSnapshot = item.Product.Price,
                Quantity = item.Quantity,
                LineTotal = item.Product.Price * item.Quantity,
            }).ToList(),
            StatusHistory =
            [
                new OrderStatusHistory
                {
                    Status = OrderStatuses.Pending,
                    ChangedAt = now,
                },
            ],
        };
        _dbContext.Orders.Add(order);

        await _dbContext.CartItems
            .Where(item => item.CartId == cart.Id)
            .ExecuteDeleteAsync(cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return await GetRequiredOrderDetailsAsync(userId, order.OrderNumber, cancellationToken);
    }

    public async Task<OrderDetailsDto?> CancelOrderAsync(
        string userId,
        string orderNumber,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        var order = await _dbContext.Orders
            .Include(candidate => candidate.Items)
            .Include(candidate => candidate.StatusHistory)
            .SingleOrDefaultAsync(
                candidate => candidate.OrderNumber == orderNumber && candidate.UserId == userId,
                cancellationToken);
        if (order is null)
        {
            return null;
        }

        if (order.Status != OrderStatuses.Pending)
        {
            throw new OrderNotCancellableException(
                "ORDER_NOT_PENDING",
                "This order can no longer be cancelled.");
        }

        order.Status = OrderStatuses.Cancelled;
        order.StatusHistory.Add(new OrderStatusHistory
        {
            OrderId = order.Id,
            Status = OrderStatuses.Cancelled,
            ChangedAt = DateTime.UtcNow,
        });

        foreach (var item in order.Items.OrderBy(item => item.ProductId))
        {
            var affectedRows = await _dbContext.Products
                .Where(product => product.Id == item.ProductId)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(product => product.Qty, product => product.Qty + item.Quantity)
                        .SetProperty(
                            product => product.StockStatus,
                            product => product.StockStatus == ProductStockStatuses.OutOfStock &&
                                product.Qty + item.Quantity > 0
                                ? ProductStockStatuses.InStock
                                : product.StockStatus),
                    cancellationToken);
            if (affectedRows != 1)
            {
                throw new InvalidOperationException(
                    $"Product {item.ProductId} for order {orderNumber} could not be found while restoring stock.");
            }
        }

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw new OrderNotCancellableException(
                "ORDER_NOT_PENDING",
                "This order can no longer be cancelled.");
        }

        await transaction.CommitAsync(cancellationToken);
        return await GetRequiredOrderDetailsAsync(userId, orderNumber, cancellationToken);
    }

    public async Task<IReadOnlyList<OrderSummaryDto>> GetOrdersAsync(
        string userId,
        CancellationToken cancellationToken = default) =>
        await _dbContext.Orders
            .AsNoTracking()
            .Where(order => order.UserId == userId)
            .OrderByDescending(order => order.CreatedAt)
            .ThenByDescending(order => order.Id)
            .Select(order => new OrderSummaryDto(
                order.OrderNumber,
                order.CreatedAt,
                order.Items.Count,
                order.Total,
                order.Status))
            .ToListAsync(cancellationToken);

    public async Task<OrderDetailsDto?> GetOrderDetailsAsync(
        string userId,
        string orderNumber,
        CancellationToken cancellationToken = default)
    {
        var order = await _dbContext.Orders
            .AsNoTracking()
            .Where(candidate => candidate.UserId == userId && candidate.OrderNumber == orderNumber)
            .Include(candidate => candidate.Items)
            .Include(candidate => candidate.StatusHistory)
            .SingleOrDefaultAsync(cancellationToken);
        return order is null ? null : ToDetails(order);
    }

    private async Task<OrderDetailsDto> GetRequiredOrderDetailsAsync(
        string userId,
        string orderNumber,
        CancellationToken cancellationToken)
    {
        var details = await GetOrderDetailsAsync(userId, orderNumber, cancellationToken);
        return details ?? throw new InvalidOperationException("The saved order could not be loaded.");
    }

    private static bool HasCompleteShippingAddress(User user) =>
        !string.IsNullOrWhiteSpace(user.ShippingFullName) &&
        !string.IsNullOrWhiteSpace(user.ShippingPhone) &&
        user.ShippingGovernorateId.HasValue &&
        !string.IsNullOrWhiteSpace(user.ShippingCity) &&
        !string.IsNullOrWhiteSpace(user.ShippingDistrict) &&
        !string.IsNullOrWhiteSpace(user.ShippingStreet) &&
        !string.IsNullOrWhiteSpace(user.ShippingBuildingNumber);

    private static OrderDetailsDto ToDetails(Order order) =>
        new(
            order.OrderNumber,
            order.CreatedAt,
            order.Status,
            order.PaymentMethod,
            order.PaymentStatus,
            order.Subtotal,
            order.ShippingFee,
            order.Total,
            order.Items
                .OrderBy(item => item.Id)
                .Select(item => new OrderItemDto(
                    item.ProductNameSnapshot,
                    item.SkuSnapshot,
                    item.UnitPriceSnapshot,
                    item.Quantity,
                    item.LineTotal))
                .ToArray(),
            new CustomerAddressDto(
                order.ShippingFullName,
                order.ShippingPhone,
                order.ShippingGovernorateId,
                order.ShippingGovernorateName,
                order.ShippingCity,
                order.ShippingDistrict,
                order.ShippingStreet,
                order.ShippingBuildingNumber,
                order.ShippingApartment,
                order.ShippingInstructions),
            order.StatusHistory
                .OrderBy(entry => entry.ChangedAt)
                .Select(entry => new OrderStatusHistoryDto(entry.Status, entry.ChangedAt))
                .ToArray());
}
