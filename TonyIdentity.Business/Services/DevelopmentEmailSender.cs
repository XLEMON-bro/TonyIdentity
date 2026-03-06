using Microsoft.Extensions.Logging;
using TonyIdentity.Business.Abstractions;

namespace TonyIdentity.Business.Services;

public class DevelopmentEmailSender : IEmailSender
{
    private readonly ILogger<DevelopmentEmailSender> _logger;

    public DevelopmentEmailSender(ILogger<DevelopmentEmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendEmailConfirmationAsync(string toEmail, string confirmationLink, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("DEV EMAIL CONFIRMATION | To: {Email} | Link: {Link}", toEmail, confirmationLink);
        return Task.CompletedTask;
    }

    public Task SendResetPasswordAsync(string toEmail, string resetLink, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("DEV RESET PASSWORD | To: {Email} | Link: {Link}", toEmail, resetLink);
        return Task.CompletedTask;
    }
}
