using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OnlineStore.Application.Auth;
using OnlineStore.Domain.Entities;
using OnlineStore.Infrastructure.Persistence;

namespace OnlineStore.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly IEmailSender _emailSender;
    private readonly IConfiguration _configuration;
    private readonly IPasswordHasher<User> _passwordHasher;

    public AuthController(
        AppDbContext dbContext,
        IEmailSender emailSender,
        IConfiguration configuration,
        IPasswordHasher<User> passwordHasher)
    {
        _dbContext = dbContext;
        _emailSender = emailSender;
        _configuration = configuration;
        _passwordHasher = passwordHasher;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var name = request.FullName?.Trim();
        var email = NormalizeEmail(request.Email);
        if (string.IsNullOrWhiteSpace(name) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "All fields are required." });
        }

        if (name.Length > 200 ||
            email.Length > 256 ||
            !new EmailAddressAttribute().IsValid(email))
        {
            return BadRequest(new { message = "Enter a valid name and email address." });
        }

        if (request.Password != request.ConfirmPassword)
        {
            return BadRequest(new { message = "Passwords do not match." });
        }

        var passwordErrors = PasswordPolicy.Validate(request.Password);
        if (passwordErrors.Count > 0)
        {
            return ValidationProblem(new ValidationProblemDetails(
                new Dictionary<string, string[]> { ["Password"] = passwordErrors.ToArray() }));
        }

        if (await _dbContext.Users.AnyAsync(user => user.Email == email))
        {
            return Conflict(new { message = "An account with this email already exists." });
        }

        var user = new User
        {
            Name = name,
            Email = email,
            Role = UserRoles.Customer,
        };
        user.HashedPassword = _passwordHasher.HashPassword(user, request.Password);

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        return StatusCode(StatusCodes.Status201Created, new { success = true, message = "Account created successfully." });
    }

    [HttpPost("login")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return Unauthorized(new { success = false, message = "Invalid email or password." });
        }

        var email = NormalizeEmail(request.Email);
        var user = await _dbContext.Users.SingleOrDefaultAsync(candidate => candidate.Email == email);
        if (user is null)
        {
            return Unauthorized(new { success = false, message = "Invalid email or password." });
        }

        var verification = _passwordHasher.VerifyHashedPassword(user, user.HashedPassword, request.Password);
        if (verification == PasswordVerificationResult.Failed)
        {
            return Unauthorized(new { success = false, message = "Invalid email or password." });
        }

        if (verification == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.HashedPassword = _passwordHasher.HashPassword(user, request.Password);
            await _dbContext.SaveChangesAsync();
        }

        return Ok(CreateAuthResponse(user));
    }

    [Authorize]
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        return NoContent();
    }

    [HttpPost("forgot-password")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var email = NormalizeEmail(request.Email);
            var user = await _dbContext.Users.SingleOrDefaultAsync(candidate => candidate.Email == email);
            if (user is not null)
            {
                var now = DateTime.UtcNow;
                var otp = Random.Shared.Next(1000, 10000).ToString("D4");
                var resetOtp = new PasswordResetOtp
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    OtpNum = otp,
                    CreatedAtUtc = now,
                    ExpiresAtUtc = now.AddMinutes(10),
                    IsUsed = false,
                    IsVerified = false,
                };

                _dbContext.PasswordResetOtps.Add(resetOtp);
                await _dbContext.SaveChangesAsync();
                await _emailSender.SendOtpEmailAsync(user.Email, otp);
            }
            else
            {
                return NotFound(new { success = false, message = "Email not found." });
            }
        }

        return Ok(new { success = true, message = "If this email is registered, you will receive a code." });
    }

    [HttpPost("verify-otp")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Otp))
        {
            return BadRequest(new { success = false, message = "Invalid or expired OTP." });
        }

        var email = NormalizeEmail(request.Email);
        var user = await _dbContext.Users.SingleOrDefaultAsync(candidate => candidate.Email == email);
        if (user is null)
        {
            return BadRequest(new { success = false, message = "Invalid or expired OTP." });
        }

        var otpRecord = await _dbContext.PasswordResetOtps
            .Where(x => x.UserId == user.Id && !x.IsUsed && x.ExpiresAtUtc > DateTime.UtcNow)
            .OrderByDescending(x => x.CreatedAtUtc)
            .FirstOrDefaultAsync();

        if (otpRecord is null || !(request.Otp == otpRecord.OtpNum))
        {
            return BadRequest(new { success = false, message = "Invalid or expired OTP." });
        }

        otpRecord.IsVerified = true;
        await _dbContext.SaveChangesAsync();

        return Ok(new { success = true, message = "OTP verified successfully." });
    }

    [HttpPost("reset-password")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Otp))
        {
            return BadRequest(new { success = false, message = "Invalid or expired OTP." });
        }

        if (string.IsNullOrWhiteSpace(request.NewPassword) ||
            string.IsNullOrWhiteSpace(request.ConfirmPassword))
        {
            return BadRequest(new { success = false, message = "New password is required." });
        }

        if (request.NewPassword != request.ConfirmPassword)
        {
            return BadRequest(new { success = false, message = "Passwords do not match." });
        }

        var passwordErrors = PasswordPolicy.Validate(request.NewPassword);
        if (passwordErrors.Count > 0)
        {
            return ValidationProblem(new ValidationProblemDetails(
                new Dictionary<string, string[]> { ["NewPassword"] = passwordErrors.ToArray() }));
        }

        var email = NormalizeEmail(request.Email);
        var user = await _dbContext.Users.SingleOrDefaultAsync(candidate => candidate.Email == email);
        if (user is null)
        {
            return BadRequest(new { success = false, message = "Invalid or expired OTP." });
        }

        var otpRecord = await _dbContext.PasswordResetOtps
            .Where(x => x.UserId == user.Id && x.IsVerified && !x.IsUsed && x.ExpiresAtUtc > DateTime.UtcNow)
            .OrderByDescending(x => x.CreatedAtUtc)
            .FirstOrDefaultAsync();

        if (otpRecord is null || !(request.Otp == otpRecord.OtpNum))
        {
            return BadRequest(new { success = false, message = "Invalid or expired OTP." });
        }

        user.HashedPassword = _passwordHasher.HashPassword(user, request.NewPassword);
        otpRecord.IsUsed = true;
        await _dbContext.SaveChangesAsync();

        return Ok(new { success = true, message = "Password reset successfully." });
    }

    private static string NormalizeEmail(string? email) => email?.Trim().ToLowerInvariant() ?? string.Empty;

    private AuthResponse CreateAuthResponse(User user)
    {
        var secret = _configuration["Jwt:Secret"] ??
            throw new InvalidOperationException("Jwt:Secret is not configured.");
        var issuer = _configuration["Jwt:Issuer"] ?? "online-store-local";
        var audience = _configuration["Jwt:Audience"] ?? "online-store-client";
        var expiryMinutes = _configuration.GetValue<int>("Jwt:ExpiryMinutes", 60);
        var expiryUtc = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Role, user.Role),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiryUtc,
            signingCredentials: creds);

        return new AuthResponse(
            new JwtSecurityTokenHandler().WriteToken(token),
            expiryUtc,
            user.Email,
            user.Name,
            user.Role);
    }

    private static string HashOtp(string otp)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(otp.Trim()));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

}
