namespace OnlineStore.Domain.Entities;

public static class OrderStatusWorkflow
{
    private static readonly string[] ForwardStatuses =
    [
        OrderStatuses.Pending,
        OrderStatuses.Confirmed,
        OrderStatuses.Processing,
        OrderStatuses.Shipped,
        OrderStatuses.Delivered,
    ];

    public static IReadOnlyList<string> Statuses => ForwardStatuses;

    public static string? GetNextStatus(string status)
    {
        var index = Array.IndexOf(ForwardStatuses, status);
        return index >= 0 && index < ForwardStatuses.Length - 1
            ? ForwardStatuses[index + 1]
            : null;
    }

    public static bool CanCancel(string status) =>
        status is OrderStatuses.Pending or OrderStatuses.Confirmed or OrderStatuses.Processing;

    public static bool CanMarkPaid(string status, string paymentStatus) =>
        status is OrderStatuses.Shipped or OrderStatuses.Delivered &&
        paymentStatus == "Pending";
}
