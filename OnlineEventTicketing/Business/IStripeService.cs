using OnlineEventTicketing.Data.Entity;
using Stripe;

namespace OnlineEventTicketing.Business
{
    public interface IStripeService
    {
        Task<string?> CreateCheckoutSessionAsync(decimal amount, string customerId, string description, string successUrl, string cancelUrl);
        Task<string?> CreateRefundAsync(string paymentIntentId, decimal amount);
        Task<Stripe.Event> ConstructWebhookEventAsync(string json, string stripeSignature, string endpointSecret);
    }
}