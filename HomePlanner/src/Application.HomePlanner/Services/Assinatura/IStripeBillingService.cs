using Domain.HomePlanner.Models.Enums;

namespace Application.HomePlanner.Services.Assinatura;

/// <summary>
/// Cliente da API do Stripe — encapsula chamadas ao boundary externo.
/// Não conhece TenantContext nem DbContext: recebe IDs e retorna URLs/IDs.
/// </summary>
public interface IStripeBillingService
{
    Task<string> CriarCheckoutSessionUrlAsync(
        PlanoAssinatura plano,
        string email,
        string? stripeCustomerId,
        Guid tenantId,
        string paisId,
        string successUrl,
        string cancelUrl,
        CancellationToken ct = default);

    Task<string> CriarCustomerPortalUrlAsync(
        string stripeCustomerId,
        string returnUrl,
        CancellationToken ct = default);

    Task<bool> CancelarAssinaturaNoFimDoPeriodoAsync(
        string stripeSubscriptionId, CancellationToken ct = default);

    Task<bool> ReativarAssinaturaAsync(
        string stripeSubscriptionId, CancellationToken ct = default);
}
