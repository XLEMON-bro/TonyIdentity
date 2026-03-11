using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TonyIdentity.Business.Interfaces
{
    public interface IEmailSender
    {
        Task SendEmailConfirmationAsync(string email, string confirmationUrl);
        Task SendPaswordResetAsync(string email, string resetUrl);
    }
}
