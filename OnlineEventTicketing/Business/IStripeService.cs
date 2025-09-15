using OnlineEventTicketing.Data.Entity;

namespace OnlineEventTicketing.Business
{
    public interface IStripeService
    {
        Task<string?> CreateCheckoutSessionAsync(decimal amount, string customerId, string description, string successUrl, string cancelUrl);
        Task<string?> CreateRefundAsync(string paymentIntentId, decimal amount);
    }
}