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
        private readonly UserManager<ApplicationUser> _userManager;

        public TicketsController(
            ITicketService ticketService, 
            IEventService eventService,
            IPaymentService paymentService,
            UserManager<ApplicationUser> userManager)
        {
            _ticketService = ticketService;
            _eventService = eventService;
            _paymentService = paymentService;
            _userManager = userManager;
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
            var viewModel = tickets.Select(t => new TicketDisplayViewModel
            {
                Id = t.Id,
                QrCode = t.QrCode,
                Price = t.Price,
                SeatNumber = t.SeatNumber,
                Status = t.Status,
                PurchaseDate = t.PurchaseDate,
                EventId = t.EventId,
                EventTitle = t.Event?.Title ?? "Unknown Event",
                EventDate = t.Event?.Date ?? DateTime.MinValue,
                EventLocation = t.Event?.Location ?? "Unknown Location",
                EventCategory = t.Event?.Category ?? "Unknown Category",
                CustomerId = t.CustomerId,
                CustomerName = t.Customer != null ? $"{t.Customer.FirstName} {t.Customer.LastName}" : "Unknown Customer"
            }).OrderByDescending(t => t.PurchaseDate).ToList();

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

            var viewModel = new PurchaseTicketViewModel
            {
                EventId = eventItem.Id,
                EventTitle = eventItem.Title,
                EventDate = eventItem.Date,
                EventLocation = eventItem.Location,
                BasePrice = eventItem.BasePrice,
                AvailableTickets = eventItem.AvailableTickets,
                TotalPrice = eventItem.BasePrice
            };

            return View(viewModel);
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

                // Calculate final price with promotion
                var finalPrice = await _eventService.CalculateTicketPriceAsync(model.EventId, model.PromotionCode);
                var totalPrice = finalPrice * model.TicketQuantity;

                // Process multiple tickets
                var purchasedTickets = new List<Ticket>();
                for (int i = 0; i < model.TicketQuantity; i++)
                {
                    var ticket = await _ticketService.PurchaseTicketAsync(model.EventId, user.Id, model.PromotionCode);
                    if (ticket != null)
                    {
                        purchasedTickets.Add(ticket);

                        // Process payment for each ticket
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

            // Reload event data for view
            var reloadedEvent = await _eventService.GetEventByIdAsync(model.EventId);
            if (reloadedEvent != null)
            {
                model.EventTitle = reloadedEvent.Title;
                model.EventDate = reloadedEvent.Date;
                model.EventLocation = reloadedEvent.Location;
                model.BasePrice = reloadedEvent.BasePrice;
                model.AvailableTickets = reloadedEvent.AvailableTickets;
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
                CustomerName = $"{ticket.Customer.FirstName} {ticket.Customer.LastName}"
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