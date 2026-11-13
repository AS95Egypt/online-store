namespace OnlineStore.Application.Auth;

public interface IEmailSender
{
    Task SendOtpEmailAsync(string toEmail, string otp, CancellationToken ct = default);
}
