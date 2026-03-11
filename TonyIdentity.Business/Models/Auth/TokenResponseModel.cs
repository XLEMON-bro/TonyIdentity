using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TonyIdentity.Business.Models.Auth
{
    public class TokenResponseModel
    {
        public string AccessToken { get; set; } = string.Empty;
        public int ExpiresInSeconds { get; set; }
        public string RefreshToken { get; set; } = string.Empty;
    }
}
