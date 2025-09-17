using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineEventTicketing.Business;
using OnlineEventTicketing.Data.Entity;
using OnlineEventTicketing.Models.ViewModels;

namespace OnlineEventTicketing.Areas.Organizer.Controllers
{
    [Area("Organizer")]
    [Authorize(Roles = "EventOrganizer")]
    public class DashboardController : Controller
    {
        private readonly IEventService _eventService;
        private readonly ITicketService _ticketService;
        private readonly IPaymentService _paymentService;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(
            IEventService eventService,
            ITicketService ticketService,
            IPaymentService paymentService,
            UserManager<ApplicationUser> userManager)
        {
            _eventService = eventService;
            _ticketService = ticketService;
            _paymentService = paymentService;
            _userManager = userManager;
        }

        // GET: /Organizer/Dashboard
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account", new { area = "Identity" });

            var events = await _eventService.GetEventsByOrganizerIdAsync(user.Id);
            var totalRevenue = await _paymentService.GetRevenueByOrganizerAsync(user.Id);

            var dashboardData = new
            {
                TotalEvents = events.Count(),
                ActiveEvents = events.Count(e => e.IsActive && e.Date > DateTime.UtcNow),
                TotalTicketsSold = events.Sum(e => e.Capacity - e.AvailableTickets),
                TotalRevenue = totalRevenue,
                UpcomingEvents = events.Where(e => e.Date > DateTime.UtcNow)
                    .OrderBy(e => e.Date)
                    .Take(5)
                    .Select(e => new EventDisplayViewModel
                    {
                        Id = e.Id,
                        Title = e.Title,
                        Date = e.Date,
                        Location = e.Location,
                        AvailableTickets = e.AvailableTickets,
                        Capacity = e.Capacity
                    }),
                RecentEvents = events.OrderByDescending(e => e.CreatedAt)
                    .Take(5)
                    .Select(e => new EventDisplayViewModel
                    {
                        Id = e.Id,
                        Title = e.Title,
                        Date = e.Date,
                        AvailableTickets = e.AvailableTickets,
                        Capacity = e.Capacity,
                        IsActive = e.IsActive
                    }),
                EventSalesData = events.Select(e => new
                {
                    EventName = e.Title,
                    TicketsSold = e.Capacity - e.AvailableTickets,
                    Revenue = (e.Capacity - e.AvailableTickets) * e.BasePrice
                }).OrderByDescending(x => x.TicketsSold).Take(10)
            };

            ViewBag.TotalEvents = dashboardData.TotalEvents;
            ViewBag.ActiveEvents = dashboardData.ActiveEvents;
            ViewBag.TotalTicketsSold = dashboardData.TotalTicketsSold;
            ViewBag.TotalRevenue = dashboardData.TotalRevenue;
            ViewBag.UpcomingEvents = dashboardData.UpcomingEvents;
            ViewBag.RecentEvents = dashboardData.RecentEvents;
            ViewBag.EventSalesData = dashboardData.EventSalesData;

            return View();
        }
    }
}