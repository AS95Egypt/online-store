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
}
