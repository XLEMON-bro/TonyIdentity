using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TonyIdentity.Business.Interfaces;

namespace TonyIdentity.Business.Services
{
    public class LogEmailSender : IEmailSender
    {
        private readonly ILogger<LogEmailSender> _logger;

        public LogEmailSender(ILogger<LogEmailSender> logger)
        {
            _logger = logger;
        }

        public Task SendEmailConfirmationAsync(string email, string confirmationUrl)
        {
            _logger.LogInformation($"Email confirmation link for {email}: {confirmationUrl}");
            return Task.CompletedTask;
        }

        public Task SendPaswordResetAsync(string email, string resetUrl)
        {
            _logger.LogInformation($"Password reset link for {email}: {resetUrl}");
            return Task.CompletedTask;
        }
    }
}
