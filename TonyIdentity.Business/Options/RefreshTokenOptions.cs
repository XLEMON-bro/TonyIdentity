using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TonyIdentity.Business.Options
{
    public class RefreshTokenOptions
    {
        public int RefreshTokenDays { get; set; }
        public string CookieName { get; set; } = "rt";
    }
}
