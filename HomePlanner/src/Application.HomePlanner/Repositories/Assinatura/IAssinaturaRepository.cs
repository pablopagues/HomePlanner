using Domain.HomePlanner.Models.SaaS.Assinatura;

namespace Application.HomePlanner.Repositories.Assinatura;

public interface IAssinaturaRepository
{
    /// <summary>Assinatura do tenant atual (rastreada, via Global Query Filter).</summary>
    Task<ConfiguracaoAssinatura?> ObterMinhaAssinaturaAsync(CancellationToken ct = default);

    /// <summary>Lookup CROSS-TENANT por StripeCustomerId — usado pelo webhook (sem TenantContext).</summary>
    Task<ConfiguracaoAssinatura?> ObterPorStripeCustomerIdAsync(string customerId, CancellationToken ct = default);

    /// <summary>Lookup CROSS-TENANT por StripeSubscriptionId — usado pelo webhook.</summary>
    Task<ConfiguracaoAssinatura?> ObterPorStripeSubscriptionIdAsync(string subscriptionId, CancellationToken ct = default);

    Task<string?> ObterPaisIdAsync(Guid tenantId, CancellationToken ct = default);
    Task<string?> ObterEmailTenantAsync(Guid tenantId, CancellationToken ct = default);

    Task<int> SalvarAsync(CancellationToken ct = default);
}
