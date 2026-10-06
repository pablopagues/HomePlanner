using Microsoft.AspNetCore.Http;

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
        if (!_tenantContext.EstaHidratado)
            _tenantContext.DefinirAPartirDe(_httpContextAccessor.HttpContext?.User);

        return Task.CompletedTask;
    }
}
