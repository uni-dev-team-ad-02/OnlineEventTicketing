using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineEventTicketing.Business;
using OnlineEventTicketing.Data.Entity;
using OnlineEventTicketing.Models.ViewModels;

namespace OnlineEventTicketing.Controllers
{
    [Authorize]
    public class TicketsController : Controller
    {
        private readonly ITicketService _ticketService;
        private readonly IEventService _eventService;
        private readonly IPaymentService _paymentService;
        private readonly IStripeService _stripeService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public TicketsController(
            ITicketService ticketService,
            IEventService eventService,
            IPaymentService paymentService,
            IStripeService stripeService,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration)
        {
            _ticketService = ticketService;
            _eventService = eventService;
            _paymentService = paymentService;
            _stripeService = stripeService;
            _userManager = userManager;
            _configuration = configuration;
        }

        // GET: Tickets
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var tickets = await _ticketService.GetTicketsByCustomerIdAsync(user.Id);
            var viewModelList = new List<TicketDisplayViewModel>();

            foreach (var ticket in tickets)
            {
                // Get payment information for this ticket
                var payments = await _paymentService.GetPaymentsByTicketIdAsync(ticket.Id);
                var latestPayment = payments.OrderByDescending(p => p.CreatedAt).FirstOrDefault();

                var ticketViewModel = new TicketDisplayViewModel
                {
                    Id = ticket.Id,
                    QrCode = ticket.QrCode,
                    Price = ticket.Price,
                    SeatNumber = ticket.SeatNumber,
                    Status = ticket.Status,
                    PurchaseDate = ticket.PurchaseDate,
                    EventId = ticket.EventId,
                    EventTitle = ticket.Event?.Title ?? "Unknown Event",
                    EventDate = ticket.Event?.Date ?? DateTime.MinValue,
                    EventLocation = ticket.Event?.Location ?? "Unknown Location",
                    EventCategory = ticket.Event?.Category ?? "Unknown Category",
                    CustomerId = ticket.CustomerId,
                    CustomerName = ticket.Customer != null ? $"{ticket.Customer.FirstName} {ticket.Customer.LastName}" : "Unknown Customer",
                    PaymentStatus = latestPayment?.Status ?? PaymentStatus.Pending,
                    PaymentMethod = latestPayment?.PaymentMethod ?? PaymentMethod.CreditCard
                };

                viewModelList.Add(ticketViewModel);
            }

            var viewModel = viewModelList.OrderByDescending(t => t.PurchaseDate).ToList();

            return View(viewModel);
        }

        // GET: Tickets/Purchase/5
        public async Task<IActionResult> Purchase(int id)
        {
            var eventItem = await _eventService.GetEventByIdAsync(id);
            if (eventItem == null)
            {
                return NotFound();
            }

            if (!eventItem.IsActive || eventItem.AvailableTickets <= 0)
            {
                TempData["Error"] = "This event is not available for ticket purchase.";
                return RedirectToAction("Details", "Events", new { id });
            }

            var isStripeEnabled = _configuration.GetValue<bool>("Stripe:IsEnabled");
            var paymentMethods = new List<string>();

            if (isStripeEnabled)
            {
                paymentMethods.Add("Stripe");
            }
            paymentMethods.Add("CreditCard");

            var viewModel = new PurchaseTicketViewModel
            {
                EventId = eventItem.Id,
                EventTitle = eventItem.Title,
                EventDate = eventItem.Date,
                EventLocation = eventItem.Location,
                BasePrice = eventItem.BasePrice,
                AvailableTickets = eventItem.AvailableTickets,
                TotalPrice = eventItem.BasePrice,
                IsStripeEnabled = isStripeEnabled,
                PaymentMethods = paymentMethods,
                PaymentMethod = isStripeEnabled ? "Stripe" : "CreditCard"
            };

            return View(viewModel);
        }

        // POST: Tickets/CreateStripeCheckout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStripeCheckout(PurchaseTicketViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var eventItem = await _eventService.GetEventByIdAsync(model.EventId);
            if (eventItem == null || !eventItem.IsActive || eventItem.AvailableTickets < model.TicketQuantity)
            {
                TempData["Error"] = "Event is not available or insufficient tickets available.";
                return RedirectToAction(nameof(Purchase), new { id = model.EventId });
            }

            var isStripeEnabled = _configuration.GetValue<bool>("Stripe:IsEnabled");
            if (!isStripeEnabled)
            {
                TempData["Error"] = "Stripe checkout is not available.";
                return RedirectToAction(nameof(Purchase), new { id = model.EventId });
            }

            var finalPrice = await _eventService.CalculateTicketPriceAsync(model.EventId, model.PromotionCode);
            var totalAmount = finalPrice * model.TicketQuantity;

            var successUrl = Url.Action("PurchaseSuccess", "Tickets", new { eventId = model.EventId, quantity = model.TicketQuantity, promotionCode = model.PromotionCode }, Request.Scheme);
            var cancelUrl = Url.Action("Purchase", "Tickets", new { id = model.EventId }, Request.Scheme);

            var checkoutUrl = await _stripeService.CreateCheckoutSessionAsync(
                totalAmount,
                user.Id,
                $"Ticket purchase for {eventItem.Title} - {model.TicketQuantity} ticket(s)",
                successUrl!,
                cancelUrl!
            );

            if (string.IsNullOrEmpty(checkoutUrl))
            {
                TempData["Error"] = "Failed to create Stripe checkout session.";
                return RedirectToAction(nameof(Purchase), new { id = model.EventId });
            }

            return Redirect(checkoutUrl);
        }

        // GET: Tickets/PurchaseSuccess
        public async Task<IActionResult> PurchaseSuccess(int eventId, int quantity, string? promotionCode)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var eventItem = await _eventService.GetEventByIdAsync(eventId);
            if (eventItem == null)
            {
                TempData["Error"] = "Event not found.";
                return RedirectToAction("Index");
            }

            // Calculate final price with promotion
            var finalPrice = await _eventService.CalculateTicketPriceAsync(eventId, promotionCode);

            // Process multiple tickets
            var purchasedTickets = new List<Ticket>();
            for (int i = 0; i < quantity; i++)
            {
                var ticket = await _ticketService.PurchaseTicketAsync(eventId, user.Id, promotionCode);
                if (ticket != null)
                {
                    purchasedTickets.Add(ticket);

                    // Create a pending payment record - webhook will update status when payment completes
                    await _paymentService.ProcessPaymentAsync(ticket.Id, user.Id, PaymentMethod.Stripe, finalPrice);
                }
            }

            if (purchasedTickets.Count == quantity)
            {
                TempData["Success"] = $"Successfully purchased {quantity} ticket(s)! Payment is being processed via Stripe.";
            }
            else if (purchasedTickets.Count > 0)
            {
                TempData["Warning"] = $"Only {purchasedTickets.Count} out of {quantity} tickets were purchased.";
            }
            else
            {
                TempData["Error"] = "Failed to process ticket purchase. Please contact support.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Tickets/Purchase
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Purchase(PurchaseTicketViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                var eventItem = await _eventService.GetEventByIdAsync(model.EventId);
                if (eventItem == null || !eventItem.IsActive || eventItem.AvailableTickets < model.TicketQuantity)
                {
                    TempData["Error"] = "Event is not available or insufficient tickets available.";
                    return RedirectToAction("Details", "Events", new { id = model.EventId });
                }

                // Check if Stripe is enabled and payment method is Stripe
                var isStripeEnabled = _configuration.GetValue<bool>("Stripe:IsEnabled");
                if (isStripeEnabled && model.PaymentMethod == "Stripe")
                {
                    // Redirect to Stripe Checkout
                    return await CreateStripeCheckout(model);
                }

                // Calculate final price with promotion for non-Stripe payments
                var finalPrice = await _eventService.CalculateTicketPriceAsync(model.EventId, model.PromotionCode);

                // Process multiple tickets for non-Stripe payments
                var purchasedTickets = new List<Ticket>();
                for (int i = 0; i < model.TicketQuantity; i++)
                {
                    var ticket = await _ticketService.PurchaseTicketAsync(model.EventId, user.Id, model.PromotionCode);
                    if (ticket != null)
                    {
                        purchasedTickets.Add(ticket);

                        // Process payment for non-Stripe methods
                        if (Enum.TryParse<PaymentMethod>(model.PaymentMethod, out var paymentMethod))
                        {
                            await _paymentService.ProcessPaymentAsync(ticket.Id, user.Id, paymentMethod, finalPrice);
                        }
                    }
                }

                if (purchasedTickets.Count == model.TicketQuantity)
                {
                    TempData["Success"] = $"Successfully purchased {model.TicketQuantity} ticket(s)!";
                    return RedirectToAction(nameof(Index));
                }
                else if (purchasedTickets.Count > 0)
                {
                    TempData["Warning"] = $"Only {purchasedTickets.Count} out of {model.TicketQuantity} tickets were purchased.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["Error"] = "Failed to purchase tickets. Please try again.";
                }
            }

            return await ReloadPurchaseView(model);
        }

        private async Task<IActionResult> ReloadPurchaseView(PurchaseTicketViewModel model)
        {
            // Reload event data for view
            var reloadedEvent = await _eventService.GetEventByIdAsync(model.EventId);
            if (reloadedEvent != null)
            {
                model.EventTitle = reloadedEvent.Title;
                model.EventDate = reloadedEvent.Date;
                model.EventLocation = reloadedEvent.Location;
                model.BasePrice = reloadedEvent.BasePrice;
                model.AvailableTickets = reloadedEvent.AvailableTickets;

                var isStripeEnabled = _configuration.GetValue<bool>("Stripe:IsEnabled");
                model.IsStripeEnabled = isStripeEnabled;

                var paymentMethods = new List<string>();
                if (isStripeEnabled)
                {
                    paymentMethods.Add("Stripe");
                }
                paymentMethods.Add("CreditCard");
                model.PaymentMethods = paymentMethods;
            }

            return View(model);
        }

        // GET: Tickets/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var ticket = await _ticketService.GetTicketByIdAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null || ticket.CustomerId != user.Id)
            {
                return Forbid();
            }

            // Get payment information for this ticket
            var payments = await _paymentService.GetPaymentsByTicketIdAsync(ticket.Id);
            var latestPayment = payments.OrderByDescending(p => p.CreatedAt).FirstOrDefault();

            var viewModel = new TicketDisplayViewModel
            {
                Id = ticket.Id,
                QrCode = ticket.QrCode,
                Price = ticket.Price,
                SeatNumber = ticket.SeatNumber,
                Status = ticket.Status,
                PurchaseDate = ticket.PurchaseDate,
                EventId = ticket.EventId,
                EventTitle = ticket.Event.Title,
                EventDate = ticket.Event.Date,
                EventLocation = ticket.Event.Location,
                EventCategory = ticket.Event.Category,
                CustomerId = ticket.CustomerId,
                CustomerName = $"{ticket.Customer.FirstName} {ticket.Customer.LastName}",
                PaymentStatus = latestPayment?.Status ?? PaymentStatus.Pending,
                PaymentMethod = latestPayment?.PaymentMethod ?? PaymentMethod.CreditCard
            };

            return View(viewModel);
        }

        // POST: Tickets/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var ticket = await _ticketService.GetTicketByIdAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null || ticket.CustomerId != user.Id)
            {
                return Forbid();
            }

            var success = await _ticketService.CancelTicketAsync(id);
            if (success)
            {
                TempData["Success"] = "Ticket cancelled successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to cancel ticket. Please try again.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Tickets/RequestRefund/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestRefund(int id)
        {
            var ticket = await _ticketService.GetTicketByIdAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null || ticket.CustomerId != user.Id)
            {
                return Forbid();
            }

            var success = await _ticketService.RefundTicketAsync(id);
            if (success)
            {
                TempData["Success"] = "Refund processed successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to process refund. Please try again.";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Tickets/QrCode/5
        public async Task<IActionResult> QrCode(int id)
        {
            var ticket = await _ticketService.GetTicketByIdAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null || ticket.CustomerId != user.Id)
            {
                return Forbid();
            }

            ViewBag.QrCode = ticket.QrCode;
            ViewBag.EventTitle = ticket.Event.Title;
            ViewBag.EventDate = ticket.Event.Date.ToString("MMM dd, yyyy 'at' h:mm tt");

            return View();
        }

        // GET: Tickets/Validate
        [AllowAnonymous]
        public IActionResult Validate()
        {
            return View();
        }

        // POST: Tickets/Validate
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Validate(string qrCode)
        {
            if (string.IsNullOrEmpty(qrCode))
            {
                ViewBag.Error = "Please enter a QR code.";
                return View();
            }

            var isValid = await _ticketService.ValidateTicketAsync(qrCode);
            if (isValid)
            {
                var ticket = await _ticketService.GetTicketByQrCodeAsync(qrCode);
                ViewBag.Success = "Ticket is valid!";
                ViewBag.EventTitle = ticket?.Event.Title;
                ViewBag.CustomerName = $"{ticket?.Customer.FirstName} {ticket?.Customer.LastName}";
            }
            else
            {
                ViewBag.Error = "Invalid or expired ticket.";
            }

            return View();
        }
    }
}