using Application.HomePlanner.Repositories.Assinatura;
using Domain.HomePlanner.Models.SaaS.Assinatura;
using Infrastructure.HomePlanner.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.HomePlanner.Repositories.Assinatura;

public class AssinaturaRepository : IAssinaturaRepository
{
    private readonly HomePlannerDbContext _db;

    public AssinaturaRepository(HomePlannerDbContext db) => _db = db;

    public Task<ConfiguracaoAssinatura?> ObterMinhaAssinaturaAsync(CancellationToken ct = default)
        => _db.ConfiguracoesAssinatura.FirstOrDefaultAsync(ct);

    public Task<ConfiguracaoAssinatura?> ObterPorStripeCustomerIdAsync(string customerId, CancellationToken ct = default)
        => _db.ConfiguracoesAssinatura
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(a => a.StripeCustomerId == customerId, ct);

    public Task<ConfiguracaoAssinatura?> ObterPorStripeSubscriptionIdAsync(string subscriptionId, CancellationToken ct = default)
        => _db.ConfiguracoesAssinatura
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(a => a.StripeSubscriptionId == subscriptionId, ct);

    public Task<string?> ObterPaisIdAsync(Guid tenantId, CancellationToken ct = default)
        => _db.Tenants
            .IgnoreQueryFilters()
            .Where(t => t.Id == tenantId)
            .Select(t => t.PaisId)
            .FirstOrDefaultAsync(ct);

    public Task<string?> ObterEmailTenantAsync(Guid tenantId, CancellationToken ct = default)
        => _db.Tenants
            .IgnoreQueryFilters()
            .Where(t => t.Id == tenantId)
            .Select(t => (string?)t.Email)
            .FirstOrDefaultAsync(ct);

    public Task<int> SalvarAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
