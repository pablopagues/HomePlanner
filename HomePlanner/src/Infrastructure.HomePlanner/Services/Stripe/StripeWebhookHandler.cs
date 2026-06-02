using Application.HomePlanner.Repositories.Assinatura;
using Domain.HomePlanner.Models.Enums;
using Domain.HomePlanner.Models.SaaS.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;

namespace Infrastructure.HomePlanner.Services.Stripe;

/// <summary>
/// Processa eventos do webhook Stripe. Idempotente — reentregas convergem ao mesmo estado.
/// Eventos: subscription.created/updated/deleted, invoice.payment_succeeded/failed.
/// </summary>
public class StripeWebhookHandler : IStripeWebhookHandler
{
    private readonly IAssinaturaRepository _repo;
    private readonly StripeOptions _options;
    private readonly ILogger<StripeWebhookHandler> _logger;

    public StripeWebhookHandler(
        IAssinaturaRepository repo,
        IOptions<StripeOptions> options,
        ILogger<StripeWebhookHandler> logger)
    {
        _repo = repo;
        _options = options.Value;
        _logger = logger;
    }

    public async Task ProcessarEventoAsync(Event stripeEvent, CancellationToken ct = default)
    {
        _logger.LogInformation("Webhook Stripe recebido: {Type} ({Id})", stripeEvent.Type, stripeEvent.Id);

        switch (stripeEvent.Type)
        {
            case "customer.subscription.created":
            case "customer.subscription.updated":
                await TratarSubscriptionAtualizadaAsync((Subscription)stripeEvent.Data.Object, ct);
                break;
            case "customer.subscription.deleted":
                await TratarSubscriptionDeletadaAsync((Subscription)stripeEvent.Data.Object, ct);
                break;
            case "invoice.payment_succeeded":
                await TratarInvoicePagaAsync((Invoice)stripeEvent.Data.Object, ct);
                break;
            case "invoice.payment_failed":
                await TratarInvoiceFalhouAsync((Invoice)stripeEvent.Data.Object, ct);
                break;
            default:
                _logger.LogDebug("Evento ignorado: {Type}", stripeEvent.Type);
                break;
        }
    }

    private async Task TratarSubscriptionAtualizadaAsync(Subscription sub, CancellationToken ct)
    {
        var assinatura = await _repo.ObterPorStripeSubscriptionIdAsync(sub.Id, ct);
        if (assinatura is null && !string.IsNullOrWhiteSpace(sub.CustomerId))
            assinatura = await _repo.ObterPorStripeCustomerIdAsync(sub.CustomerId, ct);

        if (assinatura is null)
        {
            _logger.LogWarning("subscription.updated {SubId} sem tenant correspondente. Ignorando.", sub.Id);
            return;
        }

        var priceId = sub.Items?.Data?.FirstOrDefault()?.Price?.Id;
        var plano = priceId != null ? _options.PlanoParaPriceId(priceId) : null;
        if (plano.HasValue) assinatura.Plano = plano.Value;

        assinatura.StripeSubscriptionId    = sub.Id;
        assinatura.StripeCustomerId         = sub.CustomerId;
        assinatura.StripePriceId            = priceId;
        assinatura.CanceladoAoFimDoPeriodo  = sub.CancelAtPeriodEnd;

        DateTime? periodEnd = sub.CurrentPeriodEnd;
        assinatura.DataExpiracao       = periodEnd;
        assinatura.DataProximaCobranca = sub.CancelAtPeriodEnd ? null : periodEnd;

        assinatura.Status = sub.Status switch
        {
            "active" or "trialing" => StatusAssinatura.Ativo,
            "past_due" or "unpaid" => StatusAssinatura.Suspenso,
            "canceled"             => StatusAssinatura.Cancelado,
            _                      => assinatura.Status,
        };

        assinatura.DataModificacao = DateTime.UtcNow;
        await _repo.SalvarAsync(ct);

        _logger.LogInformation("Assinatura atualizada via webhook: tenant {TenantId}, plano {Plano}, status {Status}.",
            assinatura.TenantId, assinatura.Plano, assinatura.Status);
    }

    private async Task TratarSubscriptionDeletadaAsync(Subscription sub, CancellationToken ct)
    {
        var assinatura = await _repo.ObterPorStripeSubscriptionIdAsync(sub.Id, ct);
        if (assinatura is null)
        {
            _logger.LogWarning("subscription.deleted {SubId} sem tenant. Ignorando.", sub.Id);
            return;
        }

        assinatura.Status              = StatusAssinatura.Cancelado;
        assinatura.DataCancelamento    = DateTime.UtcNow;
        assinatura.DataProximaCobranca = null;
        assinatura.DataModificacao     = DateTime.UtcNow;
        await _repo.SalvarAsync(ct);

        _logger.LogInformation("Assinatura cancelada (subscription.deleted): tenant {TenantId}.", assinatura.TenantId);
    }

    private async Task TratarInvoicePagaAsync(Invoice invoice, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(invoice.CustomerId)) return;

        var assinatura = await _repo.ObterPorStripeCustomerIdAsync(invoice.CustomerId, ct);
        if (assinatura is null) return;

        if (assinatura.Status == StatusAssinatura.Suspenso)
            assinatura.Status = StatusAssinatura.Ativo;

        assinatura.DataModificacao = DateTime.UtcNow;
        await _repo.SalvarAsync(ct);

        _logger.LogInformation("Invoice paga: tenant {TenantId}, valor {Total}.",
            assinatura.TenantId, invoice.AmountPaid / 100m);
    }

    private async Task TratarInvoiceFalhouAsync(Invoice invoice, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(invoice.CustomerId)) return;

        var assinatura = await _repo.ObterPorStripeCustomerIdAsync(invoice.CustomerId, ct);
        if (assinatura is null) return;

        assinatura.Status          = StatusAssinatura.Suspenso;
        assinatura.DataModificacao = DateTime.UtcNow;
        await _repo.SalvarAsync(ct);

        _logger.LogWarning("Invoice falhou: tenant {TenantId} marcado como suspenso.", assinatura.TenantId);
    }
}
