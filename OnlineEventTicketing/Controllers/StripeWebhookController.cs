using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineEventTicketing.Business;
using Stripe;

namespace OnlineEventTicketing.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class StripeWebhookController : ControllerBase
    {
        private readonly IStripeService _stripeService;
        private readonly IPaymentService _paymentService;
        private readonly ITicketService _ticketService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<StripeWebhookController> _logger;

        public StripeWebhookController(
            IStripeService stripeService,
            IPaymentService paymentService,
            ITicketService ticketService,
            IConfiguration configuration,
            ILogger<StripeWebhookController> logger)
        {
            _stripeService = stripeService;
            _paymentService = paymentService;
            _ticketService = ticketService;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Handle()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var endpointSecret = _configuration["Stripe:WebhookSecret"];

            try
            {
                var stripeEvent = await _stripeService.ConstructWebhookEventAsync(json, Request.Headers["Stripe-Signature"], endpointSecret);

                _logger.LogInformation("Received Stripe webhook: {EventType} with ID: {EventId}", stripeEvent.Type, stripeEvent.Id);

                switch (stripeEvent.Type)
                {
                    case "checkout.session.completed":
                        await HandleCheckoutSessionCompleted(stripeEvent);
                        break;

                    case "payment_intent.succeeded":
                        await HandlePaymentIntentSucceeded(stripeEvent);
                        break;

                    case "payment_intent.payment_failed":
                        await HandlePaymentIntentFailed(stripeEvent);
                        break;

                    case "charge.dispute.created":
                        await HandleChargeDisputed(stripeEvent);
                        break;

                    default:
                        _logger.LogInformation("Unhandled event type: {EventType}", stripeEvent.Type);
                        break;
                }

                return Ok();
            }
            catch (StripeException e)
            {
                _logger.LogError(e, "Stripe webhook signature verification failed");
                return BadRequest();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error processing Stripe webhook");
                return StatusCode(500);
            }
        }

        private async Task HandleCheckoutSessionCompleted(Stripe.Event stripeEvent)
        {
            var session = stripeEvent.Data.Object as Stripe.Checkout.Session;
            if (session == null) return;

            _logger.LogInformation("Processing checkout.session.completed for session: {SessionId}", session.Id);

            var customerId = session.Metadata?.GetValueOrDefault("customer_id");
            var description = session.Metadata?.GetValueOrDefault("description");

            if (string.IsNullOrEmpty(customerId))
            {
                _logger.LogWarning("No customer_id found in session metadata for session: {SessionId}", session.Id);
                return;
            }

            try
            {
                // Find pending payment by customer and amount
                var payment = await _paymentService.GetPendingPaymentByCustomerAndAmountAsync(
                    customerId,
                    (decimal)(session.AmountTotal ?? 0) / 100);

                if (payment != null)
                {
                    await _paymentService.UpdatePaymentStatusAsync(payment.Id, Data.Entity.PaymentStatus.Completed);
                    payment.StripePaymentIntentId = session.PaymentIntentId;
                    await _paymentService.UpdatePaymentAsync(payment);

                    _logger.LogInformation("Payment {PaymentId} marked as completed for session: {SessionId}",
                        payment.Id, session.Id);
                }
                else
                {
                    _logger.LogWarning("No pending payment found for customer {CustomerId} with amount {Amount}",
                        customerId, (decimal)(session.AmountTotal ?? 0) / 100);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error updating payment for completed checkout session: {SessionId}", session.Id);
            }
        }

        private async Task HandlePaymentIntentSucceeded(Stripe.Event stripeEvent)
        {
            var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
            if (paymentIntent == null) return;

            _logger.LogInformation("Processing payment_intent.succeeded for intent: {PaymentIntentId}", paymentIntent.Id);

            try
            {
                var payment = await _paymentService.GetPaymentByStripePaymentIntentIdAsync(paymentIntent.Id);
                if (payment != null && payment.Status == Data.Entity.PaymentStatus.Pending)
                {
                    await _paymentService.UpdatePaymentStatusAsync(payment.Id, Data.Entity.PaymentStatus.Completed);
                    _logger.LogInformation("Payment {PaymentId} marked as completed for PaymentIntent: {PaymentIntentId}",
                        payment.Id, paymentIntent.Id);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error updating payment for succeeded payment intent: {PaymentIntentId}", paymentIntent.Id);
            }
        }

        private async Task HandlePaymentIntentFailed(Stripe.Event stripeEvent)
        {
            var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
            if (paymentIntent == null) return;

            _logger.LogInformation("Processing payment_intent.payment_failed for intent: {PaymentIntentId}", paymentIntent.Id);

            try
            {
                var payment = await _paymentService.GetPaymentByStripePaymentIntentIdAsync(paymentIntent.Id);
                if (payment != null && payment.Status == Data.Entity.PaymentStatus.Pending)
                {
                    await _paymentService.UpdatePaymentStatusAsync(payment.Id, Data.Entity.PaymentStatus.Failed);
                    _logger.LogInformation("Payment {PaymentId} marked as failed for PaymentIntent: {PaymentIntentId}",
                        payment.Id, paymentIntent.Id);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error updating payment for failed payment intent: {PaymentIntentId}", paymentIntent.Id);
            }
        }

        private async Task HandleChargeDisputed(Stripe.Event stripeEvent)
        {
            var charge = stripeEvent.Data.Object as Charge;
            if (charge == null) return;

            _logger.LogInformation("Processing charge.dispute.created for charge: {ChargeId}", charge.Id);

            try
            {
                var payment = await _paymentService.GetPaymentByStripePaymentIntentIdAsync(charge.PaymentIntentId);
                if (payment != null)
                {
                    await _paymentService.UpdatePaymentStatusAsync(payment.Id, Data.Entity.PaymentStatus.Failed);
                    _logger.LogInformation("Payment {PaymentId} marked as failed due to dispute for Charge: {ChargeId}",
                        payment.Id, charge.Id);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error updating payment for disputed charge: {ChargeId}", charge.Id);
            }
        }
    }
}