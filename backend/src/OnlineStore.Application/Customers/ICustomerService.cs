namespace OnlineStore.Application.Customers;

public interface ICustomerService
{
    Task<CustomerServiceResult<CustomerProfileResponse>> GetProfileAsync(string userId);

    Task<CustomerServiceResult<CustomerProfileResponse>> UpdateProfileAsync(
        string userId,
        UpdateCustomerProfileRequest request);

    Task<CustomerServiceResult<bool>> ChangePasswordAsync(string userId, ChangePasswordRequest request);
}

public sealed record CustomerServiceResult<T>(
    T? Value = default,
    IReadOnlyDictionary<string, string[]>? ValidationErrors = null,
    string? ErrorMessage = null,
    bool NotFound = false)
{
    public bool Succeeded => ValidationErrors is null && ErrorMessage is null && !NotFound;
}
