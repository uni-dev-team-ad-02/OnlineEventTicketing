using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineEventTicketing.Business;
using OnlineEventTicketing.Data.Entity;
using OnlineEventTicketing.Models.ViewModels;

namespace OnlineEventTicketing.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
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

        // GET: /Admin/Dashboard
        public async Task<IActionResult> Index()
        {
            var allEvents = await _eventService.GetAllEventsIncludingInactiveAsync();
            var allTickets = await _ticketService.GetAllTicketsAsync();
            var allPayments = await _paymentService.GetAllPaymentsAsync();
            var totalRevenue = await _paymentService.GetTotalRevenueAsync();
            var totalUsers = _userManager.Users.Count();

            // Convert recent events to ViewModels
            var recentEventsViewModels = allEvents.OrderByDescending(e => e.CreatedAt).Take(10)
                .Select(e => new EventDisplayViewModel
                {
                    Id = e.Id,
                    Title = e.Title,
                    Description = e.Description,
                    Date = e.Date,
                    Location = e.Location,
                    Category = e.Category,
                    Capacity = e.Capacity,
                    AvailableTickets = e.AvailableTickets,
                    BasePrice = e.BasePrice,
                    ImageUrl = e.ImageUrl,
                    IsActive = e.IsActive,
                    OrganizerId = e.OrganizerId,
                    OrganizerName = e.Organizer != null ? $"{e.Organizer.FirstName} {e.Organizer.LastName}" : "Unknown",
                    CreatedAt = e.CreatedAt
                }).ToList();

            var dashboardData = new
            {
                TotalEvents = allEvents.Count(),
                TotalTicketsSold = allTickets.Count(),
                TotalRevenue = totalRevenue,
                TotalUsers = totalUsers,
                ActiveEvents = allEvents.Count(e => e.IsActive && e.Date > DateTime.UtcNow),
                PendingPayments = allPayments.Count(p => p.Status == PaymentStatus.Pending),
                RecentEvents = recentEventsViewModels,
                RecentTickets = allTickets.OrderByDescending(t => t.CreatedAt).Take(10),
                TopCategories = allEvents.GroupBy(e => e.Category)
                    .OrderByDescending(g => g.Count())
                    .Take(5)
                    .Select(g => new { Category = g.Key, Count = g.Count() }),
                MonthlyRevenue = allPayments
                    .Where(p => p.Status == PaymentStatus.Completed && p.PaymentDate >= DateTime.UtcNow.AddMonths(-6))
                    .GroupBy(p => new { p.PaymentDate.Year, p.PaymentDate.Month })
                    .Select(g => new { Month = $"{g.Key.Year}-{g.Key.Month:00}", Revenue = g.Sum(p => p.Amount) })
                    .OrderBy(x => x.Month)
            };

            ViewBag.TotalEvents = dashboardData.TotalEvents;
            ViewBag.TotalTicketsSold = dashboardData.TotalTicketsSold;
            ViewBag.TotalRevenue = dashboardData.TotalRevenue;
            ViewBag.TotalUsers = dashboardData.TotalUsers;
            ViewBag.ActiveEvents = dashboardData.ActiveEvents;
            ViewBag.PendingPayments = dashboardData.PendingPayments;
            ViewBag.RecentEvents = dashboardData.RecentEvents;
            ViewBag.RecentTickets = dashboardData.RecentTickets;
            ViewBag.TopCategories = dashboardData.TopCategories;
            ViewBag.MonthlyRevenue = dashboardData.MonthlyRevenue;

            return View();
        }
    }
}