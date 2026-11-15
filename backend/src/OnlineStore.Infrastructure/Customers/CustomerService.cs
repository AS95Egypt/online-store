using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OnlineStore.Application.Auth;
using OnlineStore.Application.Customers;
using OnlineStore.Domain.Entities;
using OnlineStore.Infrastructure.Persistence;

namespace OnlineStore.Infrastructure.Customers;

public class CustomerService : ICustomerService
{
    private readonly AppDbContext _dbContext;
    private readonly IPasswordHasher<User> _passwordHasher;

    public CustomerService(AppDbContext dbContext, IPasswordHasher<User> passwordHasher)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
    }

    public async Task<CustomerServiceResult<CustomerProfileResponse>> GetProfileAsync(string userId)
    {
        var user = await FindUserAsync(userId);
        return user is null
            ? new CustomerServiceResult<CustomerProfileResponse>(NotFound: true)
            : new CustomerServiceResult<CustomerProfileResponse>(ToProfileResponse(user));
    }

    public async Task<CustomerServiceResult<CustomerProfileResponse>> UpdateProfileAsync(
        string userId,
        UpdateCustomerProfileRequest request)
    {
        var user = await FindUserAsync(userId);
        if (user is null)
        {
            return new CustomerServiceResult<CustomerProfileResponse>(NotFound: true);
        }

        var name = request.Name?.Trim();
        if (string.IsNullOrEmpty(name))
        {
            return new CustomerServiceResult<CustomerProfileResponse>(
                ValidationErrors: new Dictionary<string, string[]>
                {
                    ["Name"] = ["Name is required."],
                });
        }

        if (name.Length > 200)
        {
            return new CustomerServiceResult<CustomerProfileResponse>(
                ValidationErrors: new Dictionary<string, string[]>
                {
                    ["Name"] = ["Name must be 200 characters or fewer."],
                });
        }

        user.Name = name;
        await _dbContext.SaveChangesAsync();

        return new CustomerServiceResult<CustomerProfileResponse>(ToProfileResponse(user));
    }

    public async Task<CustomerServiceResult<bool>> ChangePasswordAsync(
        string userId,
        ChangePasswordRequest request)
    {
        var user = await FindUserAsync(userId);
        if (user is null)
        {
            return new CustomerServiceResult<bool>(NotFound: true);
        }

        if (string.IsNullOrWhiteSpace(request.CurrentPassword) ||
            string.IsNullOrWhiteSpace(request.NewPassword) ||
            string.IsNullOrWhiteSpace(request.ConfirmPassword))
        {
            return new CustomerServiceResult<bool>(ErrorMessage: "All password fields are required.");
        }

        if (request.NewPassword != request.ConfirmPassword)
        {
            return new CustomerServiceResult<bool>(ErrorMessage: "Passwords do not match.");
        }

        var passwordErrors = PasswordPolicy.Validate(request.NewPassword);
        if (passwordErrors.Count > 0)
        {
            return new CustomerServiceResult<bool>(
                ValidationErrors: new Dictionary<string, string[]>
                {
                    ["NewPassword"] = passwordErrors.ToArray(),
                });
        }

        if (_passwordHasher.VerifyHashedPassword(user, user.HashedPassword, request.CurrentPassword) ==
            PasswordVerificationResult.Failed)
        {
            return new CustomerServiceResult<bool>(ErrorMessage: "Current password is incorrect.");
        }

        user.HashedPassword = _passwordHasher.HashPassword(user, request.NewPassword);
        await _dbContext.SaveChangesAsync();

        return new CustomerServiceResult<bool>(true);
    }

    public async Task<CustomerServiceResult<CustomerAddressDto>> GetAddressAsync(string userId)
    {
        var user = await _dbContext.Users
            .AsNoTracking()
            .Include(candidate => candidate.ShippingGovernorate)
            .SingleOrDefaultAsync(candidate => candidate.Id == userId);
        if (user is null)
        {
            return new CustomerServiceResult<CustomerAddressDto>(NotFound: true);
        }

        return new CustomerServiceResult<CustomerAddressDto>(ToAddressResponse(user));
    }

    public async Task<CustomerServiceResult<CustomerAddressDto>> UpdateAddressAsync(
        string userId,
        UpdateCustomerAddressRequest request)
    {
        var errors = new Dictionary<string, string[]>();
        ValidateAddressField(request.FullName, nameof(request.FullName), 200, errors);
        ValidateAddressField(request.Phone, nameof(request.Phone), 30, errors);
        ValidateAddressField(request.City, nameof(request.City), 150, errors);
        ValidateAddressField(request.District, nameof(request.District), 150, errors);
        ValidateAddressField(request.Street, nameof(request.Street), 250, errors);
        ValidateAddressField(request.BuildingNumber, nameof(request.BuildingNumber), 50, errors);
        ValidateOptionalAddressField(request.Apartment, nameof(request.Apartment), 100, errors);
        ValidateOptionalAddressField(request.Instructions, nameof(request.Instructions), 1000, errors);

        var governorateExists = await _dbContext.Governorates
            .AnyAsync(governorate => governorate.Id == request.GovernorateId);
        if (!governorateExists)
        {
            errors[nameof(request.GovernorateId)] = ["Select a valid governorate."];
        }

        if (errors.Count > 0)
        {
            return new CustomerServiceResult<CustomerAddressDto>(ValidationErrors: errors);
        }

        var user = await _dbContext.Users
            .SingleOrDefaultAsync(candidate => candidate.Id == userId);
        if (user is null)
        {
            return new CustomerServiceResult<CustomerAddressDto>(NotFound: true);
        }

        user.ShippingFullName = request.FullName.Trim();
        user.ShippingPhone = request.Phone.Trim();
        user.ShippingGovernorateId = request.GovernorateId;
        user.ShippingCity = request.City.Trim();
        user.ShippingDistrict = request.District.Trim();
        user.ShippingStreet = request.Street.Trim();
        user.ShippingBuildingNumber = request.BuildingNumber.Trim();
        user.ShippingApartment = string.IsNullOrWhiteSpace(request.Apartment) ? null : request.Apartment.Trim();
        user.ShippingInstructions = string.IsNullOrWhiteSpace(request.Instructions) ? null : request.Instructions.Trim();
        await _dbContext.SaveChangesAsync();

        var governorate = await _dbContext.Governorates
            .AsNoTracking()
            .SingleAsync(candidate => candidate.Id == request.GovernorateId);
        return new CustomerServiceResult<CustomerAddressDto>(
            ToAddressResponse(user, governorate.Name));
    }

    private Task<User?> FindUserAsync(string userId) =>
        _dbContext.Users.SingleOrDefaultAsync(user => user.Id == userId);

    private static CustomerProfileResponse ToProfileResponse(User user) =>
        new(user.Id, user.Name, user.Email, user.Role);

    private static CustomerAddressDto ToAddressResponse(User user, string? governorateName = null) =>
        new(
            user.ShippingFullName,
            user.ShippingPhone,
            user.ShippingGovernorateId,
            governorateName ?? user.ShippingGovernorate?.Name,
            user.ShippingCity,
            user.ShippingDistrict,
            user.ShippingStreet,
            user.ShippingBuildingNumber,
            user.ShippingApartment,
            user.ShippingInstructions);

    private static void ValidateAddressField(
        string? value,
        string field,
        int maxLength,
        IDictionary<string, string[]> errors)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors[field] = [$"{field} is required."];
        }
        else if (value.Trim().Length > maxLength)
        {
            errors[field] = [$"{field} must be {maxLength} characters or fewer."];
        }
    }

    private static void ValidateOptionalAddressField(
        string? value,
        string field,
        int maxLength,
        IDictionary<string, string[]> errors)
    {
        if (value?.Trim().Length > maxLength)
        {
            errors[field] = [$"{field} must be {maxLength} characters or fewer."];
        }
    }
}
