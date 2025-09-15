using Stripe;
using Stripe.Checkout;
using OnlineEventTicketing.Data.Entity;

namespace OnlineEventTicketing.Business
{
    public class StripeService : IStripeService
    {
        private readonly RefundService _refundService;
        private readonly SessionService _sessionService;
        private readonly ILogger<StripeService> _logger;

        public StripeService(IConfiguration configuration, ILogger<StripeService> logger)
        {
            StripeConfiguration.ApiKey = configuration["Stripe:SecretKey"];
            _refundService = new RefundService();
            _sessionService = new SessionService();
            _logger = logger;
        }

        public async Task<string?> CreateCheckoutSessionAsync(decimal amount, string customerId, string description, string successUrl, string cancelUrl)
        {
            try
            {
                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string> { "card" },
                    LineItems = new List<SessionLineItemOptions>
                    {
                        new SessionLineItemOptions
                        {
                            PriceData = new SessionLineItemPriceDataOptions
                            {
                                UnitAmount = (long)(amount * 100), // Convert to cents
                                Currency = "usd",
                                ProductData = new SessionLineItemPriceDataProductDataOptions
                                {
                                    Name = "Event Ticket",
                                    Description = description,
                                },
                            },
                            Quantity = 1,
                        },
                    },
                    Mode = "payment",
                    SuccessUrl = successUrl,
                    CancelUrl = cancelUrl,
                    Metadata = new Dictionary<string, string>
                    {
                        { "customer_id", customerId },
                        { "description", description }
                    }
                };

                var session = await _sessionService.CreateAsync(options);
                return session.Url;
            }
            catch (StripeException)
            {
                return null;
            }
        }

        public async Task<string?> CreateRefundAsync(string paymentIntentId, decimal amount)
        {
            try
            {
                var options = new RefundCreateOptions
                {
                    PaymentIntent = paymentIntentId,
                    Amount = (long)(amount * 100) // Convert to cents
                };

                var refund = await _refundService.CreateAsync(options);
                return refund.Id;
            }
            catch (StripeException)
            {
                return null;
            }
        }

        public async Task<Stripe.Event> ConstructWebhookEventAsync(string json, string stripeSignature, string endpointSecret)
        {
            try
            {
                var stripeEvent = EventUtility.ConstructEvent(json, stripeSignature, endpointSecret, throwOnApiVersionMismatch: false);

                _logger.LogInformation("Successfully processed Stripe webhook - Event: {EventType}, API Version: {ApiVersion}",
                    stripeEvent.Type, stripeEvent.ApiVersion);

                return await Task.FromResult(stripeEvent);
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Webhook signature verification failed");
                throw new StripeException($"Webhook signature verification failed: {ex.Message}");
            }
        }
    }
}