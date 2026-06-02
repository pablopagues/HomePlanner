using Microsoft.AspNetCore.Http;

namespace Application.HomePlanner.Middleware;

/// <summary>
/// Redireciona usuários autenticados cujo onboarding não foi concluído para /onboarding.
/// Roda DEPOIS de UseAuthentication e UseTenantContext, ANTES de UseAuthorization.
/// </summary>
public class OnboardingRequiredMiddleware
{
    private readonly RequestDelegate _next;

    private static readonly string[] _caminhosLivres =
    [
        "/onboarding",
        "/set-lang",
        "/health",
        "/Identity",
        "/_blazor",
        "/_framework",
        "/_content",
        "/css",
        "/js",
        "/images",
        "/favicon",
    ];

    public OnboardingRequiredMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(
        HttpContext context,
        TenantContext tenantContext,
        IOnboardingStatusReader statusReader)
    {
        // Não autenticado → segue (será barrado pela autorização das páginas)
        if (context.User.Identity?.IsAuthenticated != true)
        {
            await _next(context);
            return;
        }

        // Sem tenant → segue (estado anômalo tratado em outro lugar)
        if (tenantContext.TenantId is null)
        {
            await _next(context);
            return;
        }

        var path = context.Request.Path.Value ?? string.Empty;
        if (_caminhosLivres.Any(c => path.StartsWith(c, StringComparison.OrdinalIgnoreCase)))
        {
            await _next(context);
            return;
        }

        var completo = await statusReader.EstaCompletoAsync(tenantContext.TenantId.Value, context.RequestAborted);
        if (!completo)
        {
            context.Response.Redirect("/onboarding");
            return;
        }

        await _next(context);
    }
}
