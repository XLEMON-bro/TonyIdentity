using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Threading.RateLimiting;
using TonyIdentity.Business.Interfaces;
using TonyIdentity.Business.Options;
using TonyIdentity.Business.Services;
using TonyIdentity.Database.Context;
using TonyIdentity.Database.Entities;

namespace TonyIdentity
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("JwtOptions"));
            builder.Services.Configure<RefreshTokenOptions>(builder.Configuration.GetSection("RefreshTokenOptions"));
            builder.Services.Configure<FrontendOptions>(builder.Configuration.GetSection("FrontendOptions"));

            var jwtOptions = builder.Configuration.GetSection("JwtOptions").Get<JwtOptions>()
                ?? throw new InvalidCastException("JwtOptions configuration is missing.");

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                      ?? throw new InvalidOperationException("DefaultConnection is missing.");

            builder.Services.AddDbContext<TonyIdentityDbContext>(options =>
            {
                options.UseNpgsql(connectionString);
            });

            builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
            {
                options.User.RequireUniqueEmail = true;

                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;

                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);

                options.SignIn.RequireConfirmedEmail = true;

                options.Tokens.AuthenticatorTokenProvider = TokenOptions.DefaultAuthenticatorProvider;
            })
            .AddEntityFrameworkStores<TonyIdentityDbContext>()
            .AddDefaultTokenProviders();

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                var keyBytes = Encoding.UTF8.GetBytes(jwtOptions.SecretKey);

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,

                    ValidateAudience = true,
                    ValidAudience = jwtOptions.Audience,

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(keyBytes),

                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),

                    RoleClaimType = System.Security.Claims.ClaimTypes.Role
                };
            });

            builder.Services.AddAuthorization();

            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
            builder.Services.AddScoped<IRefreshSessionService, RefreshSessionService>();
            builder.Services.AddScoped<ITwoFactorChallengeService, TwoFactorChallengeService>();
            builder.Services.AddScoped<IEmailSender, LogEmailSender>();

            builder.Services.AddRateLimiter(options =>
            {
                options.AddFixedWindowLimiter("auth-policy", limiteroptions =>
                {
                    limiteroptions.PermitLimit = 20;
                    limiteroptions.Window = TimeSpan.FromSeconds(60);
                    limiteroptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    limiteroptions.QueueLimit = 0;
                });

                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            });

            var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("tonyReact", corsBuilder =>
                {
                    corsBuilder.WithExposedHeaders(allowedOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });

            var app = builder.Build();

            app.UseHttpsRedirection();

            app.UseCors("tonyReact");

            app.UseRateLimiter();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.MapGet("/health", () => Results.Ok(new { Status = "Healthy" }));

            app.Run();
        }
    }
}
