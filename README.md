# TonyIdentity (.NET 8, 3-layer architecture)

## Project structure
- `TonyIdentity.API` - Controllers, request/response DTOs, middleware, HTTP setup.
- `TonyIdentity.Business` - Auth orchestration, token creation/rotation, signing key abstraction, email abstraction.
- `TonyIdentity.Database` - EF Core DbContext, Identity user entity, refresh token entity/configuration.

## NuGet packages
### TonyIdentity.API
- Microsoft.AspNetCore.Authentication.JwtBearer
- Microsoft.AspNetCore.Identity.EntityFrameworkCore
- Microsoft.EntityFrameworkCore.Design
- Npgsql.EntityFrameworkCore.PostgreSQL
- Swashbuckle.AspNetCore

### TonyIdentity.Business
- System.IdentityModel.Tokens.Jwt

### TonyIdentity.Database
- Microsoft.AspNetCore.Identity.EntityFrameworkCore
- Microsoft.EntityFrameworkCore
- Npgsql.EntityFrameworkCore.PostgreSQL

## Configuration notes
- JWT issuer: `https://identity.tonysecurity.com`
- JWT audience: `tonyfood-api`
- Access token lifetime: 5 minutes
- Refresh token lifetime: 2 days
- Connection string configured for local PostgreSQL in `appsettings.json`
- Store real secrets in user secrets / secret store (especially `Jwt:PrivateKeyPem`)

## Migrations (run locally)
```bash
dotnet tool install --global dotnet-ef
dotnet ef migrations add InitialIdentity --project TonyIdentity.Database --startup-project TonyIdentity.API --output-dir Migrations
dotnet ef database update --project TonyIdentity.Database --startup-project TonyIdentity.API
```

## What to build first (recommended order)
1. Database foundations (`ApplicationUser`, `RefreshToken`, `ApplicationDbContext`, EF configs).
2. Identity + PostgreSQL wiring in API `Program.cs`.
3. JWT signing abstraction + JWKS endpoint.
4. Register / confirm-email / login endpoints.
5. Refresh rotation + logout.
6. Forgot/reset password + change password.
7. MFA setup and enable/disable endpoints.
8. Security hardening (rate limiting, lockout, CORS, problem details, logs).
9. Production hardening pass (replace local signing provider with Key Vault provider).

## Production note
`ISigningKeyProvider` is intentionally abstracted so `LocalDevelopmentSigningKeyProvider` can be replaced with an Azure Key Vault backed provider without changing token issuing controllers/services.
