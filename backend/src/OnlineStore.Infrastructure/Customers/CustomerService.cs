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

    private Task<User?> FindUserAsync(string userId) =>
        _dbContext.Users.SingleOrDefaultAsync(user => user.Id == userId);

    private static CustomerProfileResponse ToProfileResponse(User user) =>
        new(user.Id, user.Name, user.Email, user.Role);
}
