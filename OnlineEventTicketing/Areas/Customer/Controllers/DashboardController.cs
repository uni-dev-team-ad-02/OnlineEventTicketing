using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineEventTicketing.Business;
using OnlineEventTicketing.Data.Entity;
using OnlineEventTicketing.Models.ViewModels;

namespace OnlineEventTicketing.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize(Roles = "Customer")]
    public class DashboardController : Controller
    {
        private readonly ITicketService _ticketService;
        private readonly IPaymentService _paymentService;
        private readonly IEventService _eventService;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(
            ITicketService ticketService,
            IPaymentService paymentService,
            IEventService eventService,
            UserManager<ApplicationUser> userManager)
        {
            _ticketService = ticketService;
            _paymentService = paymentService;
            _eventService = eventService;
            _userManager = userManager;
        }

        // GET: /Customer/Dashboard
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account", new { area = "Identity" });

            var tickets = await _ticketService.GetTicketsByCustomerIdAsync(user.Id);
            var payments = await _paymentService.GetPaymentsByCustomerIdAsync(user.Id);
            var upcomingEvents = await _eventService.GetUpcomingEventsAsync();

            var dashboardData = new
            {
                TotalTickets = tickets.Count(),
                ActiveTickets = tickets.Count(t => t.Status == TicketStatus.Active),
                UpcomingEvents = tickets.Where(t => t.Event.Date > DateTime.UtcNow && t.Status == TicketStatus.Active).Count(),
                TotalSpent = payments.Where(p => p.Status == PaymentStatus.Completed).Sum(p => p.Amount),
                LoyaltyPoints = user.LoyaltyPoints,
                RecentTickets = tickets.OrderByDescending(t => t.CreatedAt)
                    .Take(5)
                    .Select(t => new TicketDisplayViewModel
                    {
                        Id = t.Id,
                        EventTitle = t.Event.Title,
                        EventDate = t.Event.Date,
                        EventLocation = t.Event.Location,
                        Price = t.Price,
                        Status = t.Status,
                        PurchaseDate = t.PurchaseDate
                    }),
                FavoriteCategories = tickets
                    .GroupBy(t => t.Event.Category)
                    .OrderByDescending(g => g.Count())
                    .Take(3)
                    .Select(g => new { Category = g.Key, Count = g.Count() }),
                RecommendedEvents = upcomingEvents
                    .Where(e => tickets.Any(t => t.Event.Category == e.Category))
                    .Take(3)
                    .Select(e => new EventDisplayViewModel
                    {
                        Id = e.Id,
                        Title = e.Title,
                        Date = e.Date,
                        Location = e.Location,
                        Category = e.Category,
                        BasePrice = e.BasePrice,
                        AvailableTickets = e.AvailableTickets
                    })
            };

            ViewBag.TotalTickets = dashboardData.TotalTickets;
            ViewBag.ActiveTickets = dashboardData.ActiveTickets;
            ViewBag.UpcomingEvents = dashboardData.UpcomingEvents;
            ViewBag.TotalSpent = dashboardData.TotalSpent;
            ViewBag.LoyaltyPoints = dashboardData.LoyaltyPoints;
            ViewBag.RecentTickets = dashboardData.RecentTickets;
            ViewBag.FavoriteCategories = dashboardData.FavoriteCategories;
            ViewBag.RecommendedEvents = dashboardData.RecommendedEvents;

            return View();
        }
    }
}