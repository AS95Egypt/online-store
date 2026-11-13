using Microsoft.Extensions.Logging;
using OnlineStore.Application.Auth;

namespace OnlineStore.Infrastructure.Auth;

public class LoggingEmailSender(ILogger<LoggingEmailSender> logger) : IEmailSender
{
    public Task SendOtpEmailAsync(string toEmail, string otp, CancellationToken ct = default)
    {
        logger.LogInformation("OTP for {Email}: {Otp} (dev stub — no real email sent)", toEmail, otp);
        return Task.CompletedTask;
    }
}
