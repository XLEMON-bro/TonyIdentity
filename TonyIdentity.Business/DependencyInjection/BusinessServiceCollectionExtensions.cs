using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TonyIdentity.Business.Abstractions;
using TonyIdentity.Business.Options;
using TonyIdentity.Business.Services;

namespace TonyIdentity.Business.DependencyInjection;

public static class BusinessServiceCollectionExtensions
{
    public static IServiceCollection AddBusinessServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddSingleton<ISigningKeyProvider, LocalDevelopmentSigningKeyProvider>();
        services.AddScoped<IEmailSender, DevelopmentEmailSender>();

        return services;
    }
}
