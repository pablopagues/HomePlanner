using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Application.HomePlanner.Middleware;

public class TenantContextMiddleware
{
    private readonly RequestDelegate _next;

    public TenantContextMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, TenantContext tenantContext)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var tenantIdStr = context.User.FindFirstValue("tenant_id");
            var usuarioId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var nome = context.User.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
            // Filho (e qualquer papel que não seja Owner/Membro) só enxerga os próprios registros.
            var ehOwner = context.User.IsInRole("Owner");
            var restrito = !ehOwner && !context.User.IsInRole("Membro");

            if (Guid.TryParse(tenantIdStr, out var tenantId))
                tenantContext.Definir(tenantId, usuarioId, nome, restrito, ehOwner);
        }

        await _next(context);
    }
}
