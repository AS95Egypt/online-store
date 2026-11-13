namespace OnlineStore.Application.Customers;

public record CustomerProfileResponse(string Id, string Name, string Email, string Role);

public record UpdateCustomerProfileRequest(string Name);

public record ChangePasswordRequest(string CurrentPassword, string NewPassword, string ConfirmPassword);
