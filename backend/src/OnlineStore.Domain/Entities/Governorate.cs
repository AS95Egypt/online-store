namespace OnlineStore.Domain.Entities;

public class Governorate
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal ShippingFee { get; set; }
    public ICollection<User> Customers { get; set; } = new List<User>();
}
