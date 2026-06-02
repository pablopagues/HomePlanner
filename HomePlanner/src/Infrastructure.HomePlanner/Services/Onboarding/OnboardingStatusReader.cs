using Application.HomePlanner.Middleware;
using Infrastructure.HomePlanner.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.HomePlanner.Services.Onboarding;

public class OnboardingStatusReader : IOnboardingStatusReader
{
    private readonly IDbContextFactory<HomePlannerDbContext> _dbFactory;

    public OnboardingStatusReader(IDbContextFactory<HomePlannerDbContext> dbFactory)
        => _dbFactory = dbFactory;

    public async Task<bool> EstaCompletoAsync(Guid tenantId, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        return await db.Tenants
            .IgnoreQueryFilters()
            .Where(t => t.Id == tenantId)
            .Select(t => t.OnboardingCompleto)
            .FirstOrDefaultAsync(ct);
    }
}
