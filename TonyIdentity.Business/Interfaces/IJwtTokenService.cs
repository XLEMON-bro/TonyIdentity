using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TonyIdentity.Database.Entities;

namespace TonyIdentity.Business.Interfaces
{
    public interface IJwtTokenService
    {
        Task<string> GenerateAccessTokenAsync(ApplicationUser user, IList<string> roles);
        string GenerateRandomToken();
        string ComputeHash(string value);
    }
}
