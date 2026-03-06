namespace TonyIdentity.API.Extensions;

public static class HttpContextExtensions
{
    public static string GetClientIp(this HttpContext httpContext)
    {
        return httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}
