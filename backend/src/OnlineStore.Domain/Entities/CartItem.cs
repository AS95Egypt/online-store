namespace OnlineStore.Domain.Entities;

public class CartItem
{
    public int Id { get; set; }
    public int CartId { get; set; }
    public Cart Cart { get; set; } = null!;
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    // Product status and price are always read from the current product record.
    public int Quantity { get; set; }
}
