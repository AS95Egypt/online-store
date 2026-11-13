using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OnlineStore.Application.Auth;
using OnlineStore.Domain.Entities;

namespace OnlineStore.Infrastructure.Persistence;

public sealed class AdminSeeder
{
    private readonly AppDbContext _dbContext;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AdminSeeder> _logger;

    public AdminSeeder(
        AppDbContext dbContext,
        IPasswordHasher<User> passwordHasher,
        IConfiguration configuration,
        ILogger<AdminSeeder> logger)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var email = _configuration["Seed:AdminEmail"]?.Trim().ToLowerInvariant();
        var password = _configuration["Seed:AdminPassword"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            _logger.LogWarning(
                "Development admin account was not seeded because Seed:AdminEmail or Seed:AdminPassword is not configured.");
            return;
        }

        if (!new EmailAddressAttribute().IsValid(email))
        {
            throw new InvalidOperationException("Seed:AdminEmail must be a valid email address.");
        }

        if (PasswordPolicy.Validate(password).Count > 0)
        {
            throw new InvalidOperationException(
                "Seed:AdminPassword does not meet the configured password policy.");
        }

        var existingUser = await _dbContext.Users
            .SingleOrDefaultAsync(user => user.Email == email, cancellationToken);
        if (existingUser is not null)
        {
            if (existingUser.Role == UserRoles.Admin)
            {
                _logger.LogInformation("Development admin account already exists for {Email}.", email);
            }
            else
            {
                _logger.LogWarning(
                    "Development admin account was not seeded because {Email} already belongs to a non-admin user.",
                    email);
            }

            return;
        }

        var admin = new User
        {
            Email = email,
            Name = "Administrator",
            Role = UserRoles.Admin,
        };
        admin.HashedPassword = _passwordHasher.HashPassword(admin, password);

        _dbContext.Users.Add(admin);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Development admin account created for {Email}.", email);
    }
}
