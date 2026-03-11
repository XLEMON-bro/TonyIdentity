using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TonyIdentity.Business.Models.Auth
{
    public class LoginResultModel
    {
        public bool RequiresTwoFactor { get; set; }
        public string? TwoFactorChallengeToken { get; set; }
        public TokenResponseModel? TokenResponse { get; set; }
    }
}
