using Application.HomePlanner.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Application.HomePlanner.Extensions;

public static class TenantContextExtensions
{
    public static IServiceCollection AddTenantContext(this IServiceCollection services)
    {
        services.AddScoped<TenantContext>();
        services.AddScoped<TenantContextAccessor>();
        return services;
    }

    public static IApplicationBuilder UseTenantContext(this IApplicationBuilder app)
        => app.UseMiddleware<TenantContextMiddleware>();

    public static IApplicationBuilder UseOnboardingRequired(this IApplicationBuilder app)
        => app.UseMiddleware<OnboardingRequiredMiddleware>();
}
