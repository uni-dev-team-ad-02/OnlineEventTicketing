using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineEventTicketing.Business;
using OnlineEventTicketing.Models.ViewModels;

namespace OnlineEventTicketing.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ReportsController : Controller
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Sales(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var report = await _reportService.GetAdminSalesReportAsync(startDate, endDate);
                return View(report);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error generating sales report: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Users(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var report = await _reportService.GetAdminUsersReportAsync(startDate, endDate);
                return View(report);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error generating users report: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Events(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var report = await _reportService.GetAdminEventsReportAsync(startDate, endDate);
                return View(report);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error generating events report: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ExportSalesReport(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var report = await _reportService.GetAdminSalesReportAsync(startDate, endDate);
                var csv = GenerateSalesCsv(report);
                var fileName = $"admin_sales_report_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", fileName);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error exporting sales report: " + ex.Message;
                return RedirectToAction("Sales");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ExportUsersReport(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var report = await _reportService.GetAdminUsersReportAsync(startDate, endDate);
                var csv = GenerateUsersCsv(report);
                var fileName = $"admin_users_report_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", fileName);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error exporting users report: " + ex.Message;
                return RedirectToAction("Users");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ExportEventsReport(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var report = await _reportService.GetAdminEventsReportAsync(startDate, endDate);
                var csv = GenerateEventsCsv(report);
                var fileName = $"admin_events_report_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", fileName);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error exporting events report: " + ex.Message;
                return RedirectToAction("Events");
            }
        }

        private string GenerateSalesCsv(AdminSalesReportViewModel report)
        {
            var csv = new System.Text.StringBuilder();
            csv.AppendLine($"Admin Sales Report - {report.FormattedReportPeriod}");
            csv.AppendLine();
            
            csv.AppendLine("Summary");
            csv.AppendLine($"Total Tickets Sold,{report.TotalTicketsSold}");
            csv.AppendLine($"Total Revenue,{report.FormattedTotalRevenue}");
            csv.AppendLine($"Average Ticket Price,{report.FormattedAverageTicketPrice}");
            csv.AppendLine($"Total Events,{report.TotalEvents}");
            csv.AppendLine($"Active Events,{report.ActiveEvents}");
            csv.AppendLine($"Total Organizers,{report.TotalOrganizers}");
            csv.AppendLine($"Active Tickets,{report.ActiveTickets}");
            csv.AppendLine($"Cancelled Tickets,{report.CancelledTickets}");
            csv.AppendLine($"Refunded Tickets,{report.RefundedTickets}");
            csv.AppendLine($"Cancellation Rate,{report.FormattedCancellationRate}");
            csv.AppendLine();

            csv.AppendLine("Top Organizers");
            csv.AppendLine("Organizer Name,Events Count,Tickets Sold,Revenue,Avg Revenue Per Event");
            foreach (var organizer in report.TopOrganizers)
            {
                csv.AppendLine($"{organizer.OrganizerName},{organizer.EventsCount},{organizer.TicketsSold},{organizer.FormattedRevenue},{organizer.FormattedAverageRevenuePerEvent}");
            }
            csv.AppendLine();

            csv.AppendLine("Category Performance");
            csv.AppendLine("Category,Events Count,Tickets Sold,Revenue,Market Share,Avg Revenue Per Event");
            foreach (var category in report.CategoryPerformance)
            {
                csv.AppendLine($"{category.Category},{category.EventsCount},{category.TicketsSold},{category.FormattedRevenue},{category.FormattedMarketShare},{category.FormattedAverageRevenuePerEvent}");
            }

            return csv.ToString();
        }

        private string GenerateUsersCsv(AdminUsersReportViewModel report)
        {
            var csv = new System.Text.StringBuilder();
            csv.AppendLine($"Admin Users Report - {report.FormattedReportPeriod}");
            csv.AppendLine();

            csv.AppendLine("Summary");
            csv.AppendLine($"Total Users,{report.TotalUsers}");
            csv.AppendLine($"Active Users,{report.ActiveUsers}");
            csv.AppendLine($"Inactive Users,{report.InactiveUsers}");
            csv.AppendLine($"Total Customers,{report.TotalCustomers}");
            csv.AppendLine($"Total Organizers,{report.TotalOrganizers}");
            csv.AppendLine($"Total Admins,{report.TotalAdmins}");
            csv.AppendLine($"New Users This Month,{report.NewUsersThisMonth}");
            csv.AppendLine($"Active Users Percentage,{report.FormattedActiveUsersPercentage}");
            csv.AppendLine();

            csv.AppendLine("Role Distribution");
            csv.AppendLine("Role,Count,Percentage");
            foreach (var role in report.RoleDistribution)
            {
                csv.AppendLine($"{role.Role},{role.Count},{role.FormattedPercentage}");
            }
            csv.AppendLine();

            csv.AppendLine("Top Customers");
            csv.AppendLine("Name,Email,Tickets Purchased,Total Spent,Loyalty Points,Registration Date");
            foreach (var customer in report.TopCustomers)
            {
                csv.AppendLine($"{customer.UserName},{customer.Email},{customer.TicketsPurchased},{customer.FormattedTotalSpent},{customer.LoyaltyPoints},{customer.FormattedRegistrationDate}");
            }
            csv.AppendLine();

            csv.AppendLine("Top Organizers");
            csv.AppendLine("Name,Email,Events Created,Tickets Sold,Total Revenue,Registration Date");
            foreach (var organizer in report.TopOrganizers)
            {
                csv.AppendLine($"{organizer.UserName},{organizer.Email},{organizer.EventsCreated},{organizer.TicketsSold},{organizer.FormattedTotalRevenue},{organizer.FormattedRegistrationDate}");
            }

            return csv.ToString();
        }

        private string GenerateEventsCsv(AdminEventsReportViewModel report)
        {
            var csv = new System.Text.StringBuilder();
            csv.AppendLine($"Admin Events Report - {report.FormattedReportPeriod}");
            csv.AppendLine();

            csv.AppendLine("Summary");
            csv.AppendLine($"Total Events,{report.TotalEvents}");
            csv.AppendLine($"Active Events,{report.ActiveEvents}");
            csv.AppendLine($"Inactive Events,{report.InactiveEvents}");
            csv.AppendLine($"Upcoming Events,{report.UpcomingEvents}");
            csv.AppendLine($"Past Events,{report.PastEvents}");
            csv.AppendLine($"Total Capacity,{report.TotalCapacity}");
            csv.AppendLine($"Total Tickets Sold,{report.TotalTicketsSold}");
            csv.AppendLine($"Overall Sell-through Rate,{report.FormattedOverallSellThroughRate}");
            csv.AppendLine($"Average Event Capacity,{report.FormattedAverageEventCapacity}");
            csv.AppendLine();

            csv.AppendLine("Top Performing Events");
            csv.AppendLine("Event Title,Organizer,Event Date,Location,Category,Capacity,Tickets Sold,Revenue,Sell-through Rate,Performance Rating");
            foreach (var evt in report.TopPerformingEvents)
            {
                csv.AppendLine($"{evt.EventTitle},{evt.OrganizerName},{evt.FormattedEventDate},{evt.Location},{evt.Category},{evt.Capacity},{evt.TicketsSold},{evt.FormattedRevenue},{evt.FormattedSellThroughRate},{evt.PerformanceRating}");
            }
            csv.AppendLine();

            csv.AppendLine("Category Analysis");
            csv.AppendLine("Category,Event Count,Total Capacity,Tickets Sold,Revenue,Sell-through Rate,Market Share,Avg Revenue Per Event");
            foreach (var category in report.CategoryAnalysis)
            {
                csv.AppendLine($"{category.Category},{category.EventCount},{category.TotalCapacity},{category.TicketsSold},{category.FormattedRevenue},{category.FormattedSellThroughRate},{category.FormattedMarketShare},{category.FormattedAverageRevenuePerEvent}");
            }
            csv.AppendLine();

            csv.AppendLine("Organizer Performance");
            csv.AppendLine("Organizer Name,Email,Events Created,Active Events,Total Capacity,Tickets Sold,Revenue,Sell-through Rate,Performance Rating");
            foreach (var organizer in report.OrganizerPerformance)
            {
                csv.AppendLine($"{organizer.OrganizerName},{organizer.Email},{organizer.EventsCreated},{organizer.ActiveEvents},{organizer.TotalCapacity},{organizer.TicketsSold},{organizer.FormattedRevenue},{organizer.FormattedSellThroughRate},{organizer.PerformanceRating}");
            }

            return csv.ToString();
        }
    }
}