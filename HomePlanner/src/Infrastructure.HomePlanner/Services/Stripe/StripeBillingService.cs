using Application.HomePlanner.Services.Assinatura;
using Domain.HomePlanner.Models.Enums;
using Domain.HomePlanner.Models.SaaS.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;
using Checkout = Stripe.Checkout;
using BillingPortal = Stripe.BillingPortal;

namespace Infrastructure.HomePlanner.Services.Stripe;

/// <summary>
/// Implementação Stripe.net da integração de billing.
/// Configura a SecretKey global no construtor (StripeConfiguration.ApiKey).
/// </summary>
public class StripeBillingService : IStripeBillingService
{
    private readonly StripeOptions _options;
    private readonly ILogger<StripeBillingService> _logger;

    public StripeBillingService(IOptions<StripeOptions> options, ILogger<StripeBillingService> logger)
    {
        _options = options.Value;
        _logger = logger;

        if (_options.IsEnabled && !string.IsNullOrWhiteSpace(_options.SecretKey))
            StripeConfiguration.ApiKey = _options.SecretKey;
    }

    public async Task<string> CriarCheckoutSessionUrlAsync(
        PlanoAssinatura plano, string email, string? stripeCustomerId,
        Guid tenantId, string paisId, string successUrl, string cancelUrl,
        CancellationToken ct = default)
    {
        GarantirHabilitado();

        var priceId = _options.PriceIdParaPlano(plano, paisId);
        if (string.IsNullOrWhiteSpace(priceId))
            throw new InvalidOperationException(
                $"PriceId não configurado para o plano {plano} (país {paisId}).");

        var options = new Checkout.SessionCreateOptions
        {
            Mode = "subscription",
            PaymentMethodTypes = ["card"],
            LineItems = [new() { Price = priceId, Quantity = 1 }],
            SuccessUrl = successUrl,
            CancelUrl = cancelUrl,
            Customer = string.IsNullOrWhiteSpace(stripeCustomerId) ? null : stripeCustomerId,
            CustomerEmail = string.IsNullOrWhiteSpace(stripeCustomerId) ? email : null,
            ClientReferenceId = tenantId.ToString(),
            SubscriptionData = new Checkout.SessionSubscriptionDataOptions
            {
                Metadata = new Dictionary<string, string>
                {
                    ["tenantId"] = tenantId.ToString(),
                    ["plano"]    = plano.ToString(),
                },
            },
        };

        var service = new Checkout.SessionService();
        var session = await service.CreateAsync(options, cancellationToken: ct);

        _logger.LogInformation("Checkout Session {SessionId} criada para tenant {TenantId}, plano {Plano}.",
            session.Id, tenantId, plano);
        return session.Url;
    }

    public async Task<string> CriarCustomerPortalUrlAsync(
        string stripeCustomerId, string returnUrl, CancellationToken ct = default)
    {
        GarantirHabilitado();
        if (string.IsNullOrWhiteSpace(stripeCustomerId))
            throw new ArgumentException("StripeCustomerId obrigatório.", nameof(stripeCustomerId));

        var options = new BillingPortal.SessionCreateOptions
        {
            Customer = stripeCustomerId,
            ReturnUrl = returnUrl,
        };

        var service = new BillingPortal.SessionService();
        var session = await service.CreateAsync(options, cancellationToken: ct);

        _logger.LogInformation("Customer Portal Session criada para customer {CustomerId}.", stripeCustomerId);
        return session.Url;
    }

    public async Task<bool> CancelarAssinaturaNoFimDoPeriodoAsync(
        string stripeSubscriptionId, CancellationToken ct = default)
    {
        GarantirHabilitado();
        if (string.IsNullOrWhiteSpace(stripeSubscriptionId)) return false;

        var service = new SubscriptionService();
        try
        {
            var atual = await service.GetAsync(stripeSubscriptionId, cancellationToken: ct);
            if (atual.Status is "canceled" or "incomplete_expired") return false;
            if (atual.CancelAtPeriodEnd) return true;

            await service.UpdateAsync(stripeSubscriptionId,
                new SubscriptionUpdateOptions { CancelAtPeriodEnd = true }, cancellationToken: ct);

            _logger.LogInformation("Subscription {Id} marcada para cancelar no fim do período.", stripeSubscriptionId);
            return true;
        }
        catch (StripeException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Subscription {Id} não encontrada no Stripe.", stripeSubscriptionId);
            return false;
        }
    }

    public async Task<bool> ReativarAssinaturaAsync(
        string stripeSubscriptionId, CancellationToken ct = default)
    {
        GarantirHabilitado();
        if (string.IsNullOrWhiteSpace(stripeSubscriptionId)) return false;

        var service = new SubscriptionService();
        try
        {
            var atual = await service.GetAsync(stripeSubscriptionId, cancellationToken: ct);
            if (atual.Status is "canceled" or "incomplete_expired") return false;
            if (!atual.CancelAtPeriodEnd) return true;

            await service.UpdateAsync(stripeSubscriptionId,
                new SubscriptionUpdateOptions { CancelAtPeriodEnd = false }, cancellationToken: ct);

            _logger.LogInformation("Subscription {Id} reativada.", stripeSubscriptionId);
            return true;
        }
        catch (StripeException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Subscription {Id} não encontrada no Stripe.", stripeSubscriptionId);
            return false;
        }
    }

    private void GarantirHabilitado()
    {
        if (!_options.IsEnabled)
            throw new InvalidOperationException("Integração com Stripe está desativada (Stripe:IsEnabled = false).");
        if (string.IsNullOrWhiteSpace(_options.SecretKey))
            throw new InvalidOperationException("Stripe:SecretKey não configurada.");
    }
}
