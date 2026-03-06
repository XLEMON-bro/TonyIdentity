namespace TonyIdentity.Business.Abstractions;

public interface IEmailSender
{
    Task SendEmailConfirmationAsync(string toEmail, string confirmationLink, CancellationToken cancellationToken = default);
    Task SendResetPasswordAsync(string toEmail, string resetLink, CancellationToken cancellationToken = default);
}
