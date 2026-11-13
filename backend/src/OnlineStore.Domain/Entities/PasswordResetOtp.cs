namespace OnlineStore.Domain.Entities;

public class PasswordResetOtp
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string OtpNum { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public bool IsUsed { get; set; }
    public bool IsVerified { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
