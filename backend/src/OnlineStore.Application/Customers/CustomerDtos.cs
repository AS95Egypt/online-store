namespace OnlineStore.Application.Customers;

public record CustomerProfileResponse(string Id, string Name, string Email, string Role);

public record UpdateCustomerProfileRequest(string Name);

public record ChangePasswordRequest(string CurrentPassword, string NewPassword, string ConfirmPassword);

public record CustomerAddressDto(
    string? FullName,
    string? Phone,
    int? GovernorateId,
    string? GovernorateName,
    string? City,
    string? District,
    string? Street,
    string? BuildingNumber,
    string? Apartment,
    string? Instructions);

public record UpdateCustomerAddressRequest(
    string FullName,
    string Phone,
    int GovernorateId,
    string City,
    string District,
    string Street,
    string BuildingNumber,
    string? Apartment,
    string? Instructions);
