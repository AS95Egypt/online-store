using System.Data;
using Microsoft.EntityFrameworkCore;
using OnlineStore.Application.Orders;
using OnlineStore.Application.Products;
using OnlineStore.Domain.Entities;
using OnlineStore.Infrastructure.Persistence;

namespace OnlineStore.Infrastructure.Orders;

public sealed class AdminOrderService : IAdminOrderService
{
    private readonly AppDbContext _dbContext;
    private readonly OrderService _orderService;

    public AdminOrderService(AppDbContext dbContext, OrderService orderService)
    {
        _dbContext = dbContext;
        _orderService = orderService;
    }

    public async Task<AdminOrderListResultDto> GetOrdersAsync(
        string? status,
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        page = page > 0 ? page : 1;
        pageSize = pageSize > 0
            ? Math.Min(pageSize, ProductPaginationDefaults.MaxPageSize)
            : ProductPaginationDefaults.PageSize;
        var skip = (int)Math.Min((long)(page - 1) * pageSize, int.MaxValue);

        var groupedCounts = await _dbContext.Orders
            .AsNoTracking()
            .GroupBy(order => order.Status)
            .Select(group => new { Status = group.Key, Count = group.Count() })
            .ToListAsync(cancellationToken);
        var countByStatus = groupedCounts.ToDictionary(item => item.Status, item => item.Count);
        var counts = new OrderStatusCountsDto(
            groupedCounts.Sum(item => item.Count),
            GetCount(countByStatus, OrderStatuses.Pending),
            GetCount(countByStatus, OrderStatuses.Confirmed),
            GetCount(countByStatus, OrderStatuses.Processing),
            GetCount(countByStatus, OrderStatuses.Shipped),
            GetCount(countByStatus, OrderStatuses.Delivered),
            GetCount(countByStatus, OrderStatuses.Cancelled));

        var orders = _dbContext.Orders
            .AsNoTracking()
            .Include(order => order.Items)
            .Include(order => order.User)
            .AsQueryable();
        if (!string.IsNullOrWhiteSpace(status) &&
            !string.Equals(status.Trim(), "All", StringComparison.OrdinalIgnoreCase))
        {
            var requestedStatus = status.Trim();
            orders = orders.Where(order => order.Status == requestedStatus);
        }

        var normalizedSearch = search?.Trim().ToLowerInvariant();
        if (!string.IsNullOrEmpty(normalizedSearch))
        {
            orders = orders.Where(order =>
                order.OrderNumber.ToLower().Contains(normalizedSearch) ||
                order.User.Name.ToLower().Contains(normalizedSearch) ||
                order.ShippingPhone.ToLower().Contains(normalizedSearch));
        }

        var totalCount = await orders.CountAsync(cancellationToken);
        var items = await orders
            .OrderByDescending(order => order.CreatedAt)
            .ThenByDescending(order => order.Id)
            .Skip(skip)
            .Take(pageSize)
            .Select(order => new AdminOrderListItemDto(
                order.Id,
                order.OrderNumber,
                ToUtcOffset(order.CreatedAt),
                order.User.Name,
                order.Items.Count,
                order.Total,
                order.Status,
                order.PaymentStatus))
            .ToListAsync(cancellationToken);

        return new AdminOrderListResultDto(items, totalCount, counts);
    }

    public async Task<AdminOrderDetailsDto?> GetOrderDetailsAsync(
        int orderId,
        CancellationToken cancellationToken = default)
    {
        var order = await _dbContext.Orders
            .AsNoTracking()
            .Include(candidate => candidate.User)
            .Include(candidate => candidate.Items)
            .Include(candidate => candidate.StatusHistory)
            .AsSplitQuery()
            .SingleOrDefaultAsync(candidate => candidate.Id == orderId, cancellationToken);
        if (order is null)
        {
            return null;
        }

        var changedByIds = order.StatusHistory
            .Where(entry => entry.ChangedByUserId != null)
            .Select(entry => entry.ChangedByUserId!)
            .Distinct()
            .ToArray();
        var changedByNames = changedByIds.Length == 0
            ? new Dictionary<string, string>()
            : await _dbContext.Users
                .AsNoTracking()
                .Where(user => changedByIds.Contains(user.Id))
                .ToDictionaryAsync(user => user.Id, user => user.Name, cancellationToken);

        return ToDetails(order, changedByNames);
    }

    public async Task<AdminOrderDetailsDto> UpdateStatusAsync(
        int orderId,
        string targetStatus,
        string adminUserId,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(
            IsolationLevel.ReadCommitted,
            cancellationToken);
        var order = await GetTrackedOrderAsync(orderId, cancellationToken);
        var nextStatus = OrderStatusWorkflow.GetNextStatus(order.Status);
        if (nextStatus is null || !string.Equals(targetStatus, nextStatus, StringComparison.Ordinal))
        {
            throw new AdminOrderConflictException(
                "INVALID_ORDER_TRANSITION",
                "The requested status is not the next valid status for this order.");
        }

        order.Status = nextStatus;
        order.StatusHistory.Add(new OrderStatusHistory
        {
            OrderId = order.Id,
            Status = nextStatus,
            ChangedAt = DateTime.UtcNow,
            ChangedByUserId = adminUserId,
        });

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw ConcurrencyConflict();
        }

        await transaction.CommitAsync(cancellationToken);
        return await GetRequiredDetailsAsync(orderId, cancellationToken);
    }

    public async Task<AdminOrderDetailsDto> CancelOrderAsync(
        int orderId,
        string adminUserId,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(
            IsolationLevel.ReadCommitted,
            cancellationToken);
        var order = await GetTrackedOrderAsync(orderId, cancellationToken, includeItems: true);
        if (!OrderStatusWorkflow.CanCancel(order.Status))
        {
            throw new AdminOrderConflictException(
                "ORDER_NOT_CANCELLABLE",
                "This order can no longer be cancelled.");
        }

        try
        {
            await _orderService.CancelOrderCoreAsync(order, adminUserId, cancellationToken);
        }
        catch (OrderNotCancellableException exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw new AdminOrderConflictException(exception.Code, exception.Message);
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw ConcurrencyConflict();
        }

        await transaction.CommitAsync(cancellationToken);
        return await GetRequiredDetailsAsync(orderId, cancellationToken);
    }

    public async Task<AdminOrderDetailsDto> MarkPaidAsync(
        int orderId,
        string adminUserId,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(
            IsolationLevel.ReadCommitted,
            cancellationToken);
        var order = await GetTrackedOrderAsync(orderId, cancellationToken);
        if (!OrderStatusWorkflow.CanMarkPaid(order.Status, order.PaymentStatus))
        {
            throw new AdminOrderConflictException(
                "ORDER_PAYMENT_NOT_ELIGIBLE",
                "This order cannot be marked as paid.");
        }

        order.PaymentStatus = "Paid";
        order.PaidAt = DateTime.UtcNow;

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw ConcurrencyConflict();
        }

        await transaction.CommitAsync(cancellationToken);
        return await GetRequiredDetailsAsync(orderId, cancellationToken);
    }

    private async Task<Order> GetTrackedOrderAsync(
        int orderId,
        CancellationToken cancellationToken,
        bool includeItems = false)
    {
        IQueryable<Order> query = _dbContext.Orders
            .Include(order => order.StatusHistory);
        if (includeItems)
        {
            query = query.Include(order => order.Items);
        }

        return await query.SingleOrDefaultAsync(order => order.Id == orderId, cancellationToken)
            ?? throw new AdminOrderNotFoundException(orderId);
    }

    private async Task<AdminOrderDetailsDto> GetRequiredDetailsAsync(
        int orderId,
        CancellationToken cancellationToken) =>
        await GetOrderDetailsAsync(orderId, cancellationToken)
            ?? throw new AdminOrderNotFoundException(orderId);

    private static AdminOrderDetailsDto ToDetails(
        Order order,
        IReadOnlyDictionary<string, string> changedByNames)
    {
        var canCancel = OrderStatusWorkflow.CanCancel(order.Status);
        var canMarkPaid = OrderStatusWorkflow.CanMarkPaid(order.Status, order.PaymentStatus);
        return new AdminOrderDetailsDto(
            order.Id,
            order.OrderNumber,
            ToUtcOffset(order.CreatedAt),
            order.Status,
            order.PaymentMethod,
            order.PaymentStatus,
            order.PaidAt is { } paidAt ? ToUtcOffset(paidAt) : null,
            order.User.Name,
            order.ShippingPhone,
            order.Subtotal,
            order.ShippingFee,
            order.Total,
            order.ShippingFullName,
            order.ShippingPhone,
            order.ShippingGovernorateName,
            order.ShippingCity,
            order.ShippingDistrict,
            order.ShippingStreet,
            order.ShippingBuildingNumber,
            order.ShippingApartment,
            order.ShippingInstructions,
            order.Items
                .OrderBy(item => item.Id)
                .Select(item => new AdminOrderItemDto(
                    item.ProductNameSnapshot,
                    item.SkuSnapshot,
                    item.UnitPriceSnapshot,
                    item.Quantity,
                    item.LineTotal))
                .ToArray(),
            order.StatusHistory
                .OrderByDescending(entry => entry.ChangedAt)
                .ThenByDescending(entry => entry.Id)
                .Select(entry => new AdminOrderStatusHistoryDto(
                    entry.Status,
                    ToUtcOffset(entry.ChangedAt),
                    entry.ChangedByUserId is null
                        ? "Customer"
                        : changedByNames.GetValueOrDefault(entry.ChangedByUserId, "Admin")))
                .ToArray(),
            new AdminOrderAvailableActionsDto(
                OrderStatusWorkflow.GetNextStatus(order.Status),
                canCancel,
                canMarkPaid));
    }

    private static DateTimeOffset ToUtcOffset(DateTime value) =>
        new(DateTime.SpecifyKind(value, DateTimeKind.Utc));

    private static int GetCount(IReadOnlyDictionary<string, int> counts, string status) =>
        counts.TryGetValue(status, out var count) ? count : 0;

    private static AdminOrderConflictException ConcurrencyConflict() =>
        new("ORDER_UPDATED", "This order was just updated.");
}
