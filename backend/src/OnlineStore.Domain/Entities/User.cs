namespace OnlineStore.Domain.Entities;

public static class UserRoles
{
    public const string Admin = "ADMIN";
    public const string Customer = "CUSTOMER";
}

public class User
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string HashedPassword { get; set; } = string.Empty;
    public string Role { get; set; } = UserRoles.Customer;
    public string? ShippingFullName { get; set; }
    public string? ShippingPhone { get; set; }
    public int? ShippingGovernorateId { get; set; }
    public Governorate? ShippingGovernorate { get; set; }
    public string? ShippingCity { get; set; }
    public string? ShippingDistrict { get; set; }
    public string? ShippingStreet { get; set; }
    public string? ShippingBuildingNumber { get; set; }
    public string? ShippingApartment { get; set; }
    public string? ShippingInstructions { get; set; }
}
