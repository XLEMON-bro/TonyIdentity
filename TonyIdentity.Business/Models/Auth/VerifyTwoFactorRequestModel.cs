using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TonyIdentity.Business.Models.Auth
{
    public class VerifyTwoFactorRequestModel
    {
        public string TwoFactorChallengeToken { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }
}
