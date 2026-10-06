using Microsoft.AspNetCore.Http;

namespace Application.HomePlanner.Middleware;

public class TenantContextMiddleware
{
    private readonly RequestDelegate _next;

    public TenantContextMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, TenantContext tenantContext)
    {
        tenantContext.DefinirAPartirDe(context.User);
        await _next(context);
    }
}
