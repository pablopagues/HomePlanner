using Domain.HomePlanner.Models.SaaS.Options;
using Infrastructure.HomePlanner.Services.Stripe;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;

namespace HomePlanner.BlazorServer.Controllers;

/// <summary>
/// Endpoint que recebe eventos webhook do Stripe.
/// [AllowAnonymous] — Stripe chama de fora. Validação HMAC obrigatória.
/// </summary>
[ApiController]
[Route("api/webhook/stripe")]
[AllowAnonymous]
public class StripeWebhookController : ControllerBase
{
    private readonly IStripeWebhookHandler _handler;
    private readonly StripeOptions _options;
    private readonly ILogger<StripeWebhookController> _logger;

    public StripeWebhookController(
        IStripeWebhookHandler handler,
        IOptions<StripeOptions> options,
        ILogger<StripeWebhookController> logger)
    {
        _handler = handler;
        _options = options.Value;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Receber(CancellationToken ct)
    {
        if (!_options.IsEnabled)
            return StatusCode(503, new { error = "Stripe integration disabled." });

        if (string.IsNullOrWhiteSpace(_options.WebhookSecret))
        {
            _logger.LogError("WebhookSecret não configurado — não é possível validar HMAC.");
            return StatusCode(500, new { error = "WebhookSecret not configured." });
        }

        // 1) Corpo cru — necessário para o HMAC
        string json;
        using (var reader = new StreamReader(HttpContext.Request.Body))
            json = await reader.ReadToEndAsync(ct);

        // 2) Valida assinatura HMAC
        var signature = Request.Headers["Stripe-Signature"].ToString();
        Event stripeEvent;
        try
        {
            stripeEvent = EventUtility.ConstructEvent(json, signature, _options.WebhookSecret);
        }
        catch (StripeException ex)
        {
            _logger.LogWarning(ex, "Webhook Stripe com assinatura inválida.");
            return BadRequest(new { error = "Invalid signature." });
        }

        // 3) Processa
        try
        {
            await _handler.ProcessarEventoAsync(stripeEvent, ct);
            return Ok(new { received = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro processando webhook {Type} ({Id}). Stripe vai reenviar.",
                stripeEvent.Type, stripeEvent.Id);
            return StatusCode(500, new { error = "Internal processing error." });
        }
    }
}
