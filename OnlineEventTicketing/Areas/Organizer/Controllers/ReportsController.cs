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
    public class ReportsController : Controller
    {
        private readonly IReportService _reportService;
        private readonly IEventService _eventService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReportsController(
            IReportService reportService,
            IEventService eventService,
            UserManager<ApplicationUser> userManager)
        {
            _reportService = reportService;
            _eventService = eventService;
            _userManager = userManager;
        }

        // GET: /Organizer/Reports
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            // Get event sales overview
            var eventSales = await _reportService.GetOrganizerEventSalesAsync(user.Id);
            
            // Get recent revenue summary (last 30 days)
            var recentRevenue = await _reportService.GetOrganizerRevenueReportAsync(user.Id, DateTime.Now.AddDays(-30), DateTime.Now);

            ViewBag.EventSales = eventSales;
            ViewBag.RecentRevenue = recentRevenue;

            return View();
        }

        // GET: /Organizer/Reports/Revenue
        public async Task<IActionResult> Revenue(DateTime? startDate, DateTime? endDate)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            // Default to last 12 months if no dates provided
            startDate ??= DateTime.Now.AddMonths(-12);
            endDate ??= DateTime.Now;

            var report = await _reportService.GetOrganizerRevenueReportAsync(user.Id, startDate, endDate);
            
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;

            return View(report);
        }

        // GET: /Organizer/Reports/Sales
        public async Task<IActionResult> Sales(DateTime? startDate, DateTime? endDate)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            // Default to last 3 months if no dates provided
            startDate ??= DateTime.Now.AddMonths(-3);
            endDate ??= DateTime.Now;

            var report = await _reportService.GetOrganizerSalesReportAsync(user.Id, startDate, endDate);
            
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;

            return View(report);
        }

        // GET: /Organizer/Reports/EventPerformance/5
        public async Task<IActionResult> EventPerformance(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            // Verify the event belongs to the organizer
            var organizerEvents = await _eventService.GetEventsByOrganizerIdAsync(user.Id);
            if (!organizerEvents.Any(e => e.Id == id))
            {
                return NotFound();
            }

            var report = await _reportService.GetEventPerformanceReportAsync(id, user.Id);
            
            if (report.EventId == 0)
            {
                return NotFound();
            }

            return View(report);
        }

        // GET: /Organizer/Reports/Promotions
        public async Task<IActionResult> Promotions()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var report = await _reportService.GetPromotionEffectivenessReportAsync(user.Id);

            return View(report);
        }

        // GET: /Organizer/Reports/EventSales
        public async Task<IActionResult> EventSales()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var eventSales = await _reportService.GetOrganizerEventSalesAsync(user.Id);

            return View(eventSales);
        }

        // POST: /Organizer/Reports/ExportRevenue
        [HttpPost]
        public async Task<IActionResult> ExportRevenue(DateTime startDate, DateTime endDate, string format = "csv")
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var report = await _reportService.GetOrganizerRevenueReportAsync(user.Id, startDate, endDate);

            if (format.ToLower() == "csv")
            {
                var csvContent = GenerateRevenueCsv(report);
                var fileName = $"revenue-report-{startDate:yyyy-MM-dd}-to-{endDate:yyyy-MM-dd}.csv";
                return File(System.Text.Encoding.UTF8.GetBytes(csvContent), "text/csv", fileName);
            }

            // Default to JSON for now
            return Json(report);
        }

        // POST: /Organizer/Reports/ExportSales
        [HttpPost]
        public async Task<IActionResult> ExportSales(DateTime startDate, DateTime endDate, string format = "csv")
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var report = await _reportService.GetOrganizerSalesReportAsync(user.Id, startDate, endDate);

            if (format.ToLower() == "csv")
            {
                var csvContent = GenerateSalesCsv(report);
                var fileName = $"sales-report-{startDate:yyyy-MM-dd}-to-{endDate:yyyy-MM-dd}.csv";
                return File(System.Text.Encoding.UTF8.GetBytes(csvContent), "text/csv", fileName);
            }

            return Json(report);
        }

        private string GenerateRevenueCsv(OrganizerRevenueReportViewModel report)
        {
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Revenue Report");
            csv.AppendLine($"Period,{report.FormattedReportPeriod}");
            csv.AppendLine($"Total Revenue,{report.TotalRevenue:C}");
            csv.AppendLine($"Total Tickets Sold,{report.TotalTicketsSold}");
            csv.AppendLine($"Total Events,{report.TotalEvents}");
            csv.AppendLine("");
            csv.AppendLine("Event Breakdown");
            csv.AppendLine("Event Title,Event Date,Revenue,Tickets Sold,Capacity,Sell-through Rate");
            
            foreach (var eventItem in report.EventBreakdown)
            {
                csv.AppendLine($"{eventItem.EventTitle},{eventItem.FormattedEventDate},{eventItem.Revenue:C},{eventItem.TicketsSold},{eventItem.Capacity},{eventItem.FormattedSellThroughRate}");
            }

            return csv.ToString();
        }

        private string GenerateSalesCsv(OrganizerSalesReportViewModel report)
        {
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Sales Report");
            csv.AppendLine($"Period,{report.FormattedReportPeriod}");
            csv.AppendLine($"Total Tickets Sold,{report.TotalTicketsSold}");
            csv.AppendLine($"Total Revenue,{report.TotalRevenue:C}");
            csv.AppendLine($"Active Tickets,{report.ActiveTickets}");
            csv.AppendLine($"Cancelled Tickets,{report.CancelledTickets}");
            csv.AppendLine("");
            csv.AppendLine("Daily Sales");
            csv.AppendLine("Date,Tickets Sold,Revenue");
            
            foreach (var day in report.DailySales)
            {
                csv.AppendLine($"{day.FormattedDate},{day.TicketsSold},{day.Revenue:C}");
            }

            return csv.ToString();
        }
    }
}