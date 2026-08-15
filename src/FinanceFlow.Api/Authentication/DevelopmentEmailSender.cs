using Microsoft.Extensions.Logging;

namespace FinanceFlow.Api.Authentication;

public interface IEmailSender
{
    Task SendVerificationAsync(string email, string token, CancellationToken cancellationToken);
    Task SendPasswordResetAsync(string email, string token, CancellationToken cancellationToken);
}

public sealed class DevelopmentEmailSender(ILogger<DevelopmentEmailSender> logger) : IEmailSender
{
    public Task SendVerificationAsync(string email, string token, CancellationToken cancellationToken)
    {
        logger.LogInformation("FinanceFlow verification token for {Email}: {Token}", email, token);
        return Task.CompletedTask;
    }

    public Task SendPasswordResetAsync(string email, string token, CancellationToken cancellationToken)
    {
        logger.LogInformation("FinanceFlow password reset token for {Email}: {Token}", email, token);
        return Task.CompletedTask;
    }
}
