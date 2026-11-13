namespace OnlineStore.Application.Auth;

public record RegisterRequest(string FullName, string Email, string Password, string ConfirmPassword);
public record LoginRequest(string Email, string Password);
public record AuthResponse(string Token, DateTime ExpiresAtUtc, string Email, string FullName, string Role);
public record ForgotPasswordRequest(string Email);
public record VerifyOtpRequest(string Email, string Otp);
public record ResetPasswordRequest(string Email, string Otp, string NewPassword, string ConfirmPassword);
