using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TonyIdentity.Business.Models.Auth
{
    public class ForgotPasswordRequestModel
    {
        public string Email { get; set; } = string.Empty;
    }
}
