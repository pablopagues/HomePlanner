using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Application.HomePlanner.Middleware;

/// <summary>
/// Garante que TenantContext esteja hidratado em contextos Blazor Server
/// onde o circuito SignalR pode criar um novo scope antes do middleware HTTP rodar.
/// </summary>
public class TenantContextAccessor
{
    private readonly TenantContext _tenantContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantContextAccessor(TenantContext tenantContext, IHttpContextAccessor httpContextAccessor)
    {
        _tenantContext = tenantContext;
        _httpContextAccessor = httpContextAccessor;
    }

    public Task GarantirHidratadoAsync()
    {
        if (_tenantContext.EstaHidratado) return Task.CompletedTask;

        var ctx = _httpContextAccessor.HttpContext;
        if (ctx?.User.Identity?.IsAuthenticated != true) return Task.CompletedTask;

        var tenantIdStr = ctx.User.FindFirstValue("tenant_id");
        var usuarioId = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var nome = ctx.User.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
        // Filho (e qualquer papel que não seja Owner/Membro) só enxerga os próprios registros.
        var ehOwner = ctx.User.IsInRole("Owner");
        var restrito = !ehOwner && !ctx.User.IsInRole("Membro");

        if (Guid.TryParse(tenantIdStr, out var tenantId))
            _tenantContext.Definir(tenantId, usuarioId, nome, restrito, ehOwner);

        return Task.CompletedTask;
    }
}
