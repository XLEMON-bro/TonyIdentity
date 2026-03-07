using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TonyIdentity.Database.Context;

namespace TonyIdentity.Database
{
    public static class DataBaseDependencyInjection
    {
        public static IServiceCollection AddDatabaseLayer(this IServiceCollection services, IConfiguration configuration) 
        {
            var connstring = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

            services.AddDbContext<IdentityAppDbContext>(options => 
            {
                options.UseNpgsql(connstring);
            });

            return services;
        }
    }
}
