using Stripe;

namespace Infrastructure.HomePlanner.Services.Stripe;

public interface IStripeWebhookHandler
{
    Task ProcessarEventoAsync(Event stripeEvent, CancellationToken ct = default);
}
