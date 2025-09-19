using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineEventTicketing.Business;
using OnlineEventTicketing.Data.Entity;
using OnlineEventTicketing.Helpers;
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
        private readonly IEventService _eventService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<StripeWebhookController> _logger;

        public StripeWebhookController(
            IStripeService stripeService,
            IPaymentService paymentService,
            ITicketService ticketService,
            IEventService eventService,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration,
            ILogger<StripeWebhookController> logger)
        {
            _stripeService = stripeService;
            _paymentService = paymentService;
            _ticketService = ticketService;
            _eventService = eventService;
            _userManager = userManager;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Handle()
        {
            try
            {
                var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
                var endpointSecret = _configuration["Stripe:WebhookSecret"];
                var stripeSignature = Request.Headers["Stripe-Signature"].FirstOrDefault();

                _logger.LogInformation("Webhook received - Body length: {BodyLength}, Has signature: {HasSignature}, Has secret: {HasSecret}",
                    json?.Length ?? 0, !string.IsNullOrEmpty(stripeSignature), !string.IsNullOrEmpty(endpointSecret));

                if (string.IsNullOrEmpty(endpointSecret))
                {
                    _logger.LogError("Stripe webhook secret not configured");
                    return BadRequest("Webhook secret not configured");
                }

                if (string.IsNullOrEmpty(stripeSignature))
                {
                    _logger.LogError("Missing Stripe-Signature header");
                    return BadRequest("Missing Stripe-Signature header");
                }

                if (string.IsNullOrEmpty(json))
                {
                    _logger.LogError("Empty webhook body received");
                    return BadRequest("Empty webhook body");
                }

                var stripeEvent = await _stripeService.ConstructWebhookEventAsync(json, stripeSignature, endpointSecret);

                _logger.LogInformation("Successfully verified Stripe webhook: {EventType} with ID: {EventId}", stripeEvent.Type, stripeEvent.Id);

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
                _logger.LogError(e, "Stripe webhook signature verification failed: {Message}", e.Message);
                return BadRequest($"Signature verification failed: {e.Message}");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error processing Stripe webhook: {Message}", e.Message);
                return StatusCode(500, $"Internal error: {e.Message}");
            }
        }

        private async Task HandleCheckoutSessionCompleted(Stripe.Event stripeEvent)
        {
            var session = stripeEvent.Data.Object as Stripe.Checkout.Session;
            if (session == null) return;

            _logger.LogInformation("Processing checkout.session.completed for session: {SessionId}", session.Id);

            var customerId = session.Metadata?.GetValueOrDefault("customer_id");
            var description = session.Metadata?.GetValueOrDefault("description");
            var paymentIdsString = session.Metadata?.GetValueOrDefault("payment_ids");

            _logger.LogInformation("Session metadata - Customer: {CustomerId}, PaymentIds: {PaymentIds}",
                customerId, paymentIdsString);

            if (string.IsNullOrEmpty(paymentIdsString))
            {
                _logger.LogWarning("No payment_ids found in session metadata for session: {SessionId}", session.Id);
                return;
            }

            try
            {
                // Parse payment IDs from metadata
                var paymentIdStrings = paymentIdsString.Split(',', StringSplitOptions.RemoveEmptyEntries);
                var paymentIds = new List<int>();

                foreach (var paymentIdString in paymentIdStrings)
                {
                    if (int.TryParse(paymentIdString.Trim(), out var paymentId))
                    {
                        paymentIds.Add(paymentId);
                    }
                }

                _logger.LogInformation("Updating {Count} payments from session metadata: [{PaymentIds}]",
                    paymentIds.Count, string.Join(", ", paymentIds));

                var updatedCount = 0;
                var successfulTickets = new List<(OnlineEventTicketing.Data.Entity.Ticket ticket, ApplicationUser user, OnlineEventTicketing.Data.Entity.Event eventItem)>();

                foreach (var paymentId in paymentIds)
                {
                    var payment = await _paymentService.GetPaymentByIdAsync(paymentId);
                    if (payment != null && payment.Status == Data.Entity.PaymentStatus.Pending)
                    {
                        await _paymentService.UpdatePaymentStatusAsync(payment.Id, Data.Entity.PaymentStatus.Completed);
                        payment.StripePaymentIntentId = session.PaymentIntentId;
                        await _paymentService.UpdatePaymentAsync(payment);

                        _logger.LogInformation("Payment {PaymentId} marked as completed", payment.Id);
                        updatedCount++;

                        // Get ticket details for email sending
                        try
                        {
                            var ticket = await _ticketService.GetTicketByIdAsync(payment.TicketId);
                            if (ticket != null)
                            {
                                var user = await _userManager.FindByIdAsync(ticket.CustomerId);
                                var eventItem = await _eventService.GetEventByIdAsync(ticket.EventId);

                                if (user != null && eventItem != null)
                                {
                                    successfulTickets.Add((ticket, user, eventItem));
                                }
                            }
                        }
                        catch (Exception emailEx)
                        {
                            _logger.LogError(emailEx, "Error preparing ticket email data for payment {PaymentId}", payment.Id);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Payment {PaymentId} not found or not pending", paymentId);
                    }
                }

                // Send ticket confirmation emails
                foreach (var (ticket, user, eventItem) in successfulTickets)
                {
                    try
                    {
                        await EmailHelper.SendTicketConfirmationEmailAsync(
                            user, ticket, eventItem, _configuration, _logger);
                        _logger.LogInformation("Ticket confirmation email sent for ticket {TicketId} to user {UserId}",
                            ticket.Id, user.Id);
                    }
                    catch (Exception emailEx)
                    {
                        _logger.LogError(emailEx, "Failed to send ticket confirmation email for ticket {TicketId} to user {UserId}",
                            ticket.Id, user.Id);
                    }
                }

                _logger.LogInformation("Successfully updated {UpdatedCount} out of {TotalCount} payments and sent {EmailCount} ticket emails for session: {SessionId}",
                    updatedCount, paymentIds.Count, successfulTickets.Count, session.Id);
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