using System.ComponentModel.DataAnnotations;

namespace OnlineEventTicketing.Models.ViewModels
{
    public class AdminSalesReportViewModel
    {
        [Display(Name = "Total Tickets Sold")]
        public int TotalTicketsSold { get; set; }

        [Display(Name = "Total Revenue")]
        public decimal TotalRevenue { get; set; }

        [Display(Name = "Total Revenue")]
        public string FormattedTotalRevenue => TotalRevenue.ToString("C");

        [Display(Name = "Total Events")]
        public int TotalEvents { get; set; }

        [Display(Name = "Active Events")]
        public int ActiveEvents { get; set; }

        [Display(Name = "Total Organizers")]
        public int TotalOrganizers { get; set; }

        [Display(Name = "Active Tickets")]
        public int ActiveTickets { get; set; }

        [Display(Name = "Cancelled Tickets")]
        public int CancelledTickets { get; set; }

        [Display(Name = "Refunded Tickets")]
        public int RefundedTickets { get; set; }

        [Display(Name = "Average Ticket Price")]
        public decimal AverageTicketPrice => TotalTicketsSold > 0 ? TotalRevenue / TotalTicketsSold : 0;

        [Display(Name = "Average Ticket Price")]
        public string FormattedAverageTicketPrice => AverageTicketPrice.ToString("C");

        [Display(Name = "Cancellation Rate")]
        public decimal CancellationRate => TotalTicketsSold > 0 ? (decimal)CancelledTickets / TotalTicketsSold * 100 : 0;

        [Display(Name = "Cancellation Rate")]
        public string FormattedCancellationRate => $"{CancellationRate:F1}%";

        [Display(Name = "Report Period")]
        public DateTime StartDate { get; set; }

        [Display(Name = "Report Period")]
        public DateTime EndDate { get; set; }

        [Display(Name = "Report Period")]
        public string FormattedReportPeriod => $"{StartDate:MMM dd, yyyy} - {EndDate:MMM dd, yyyy}";

        [Display(Name = "Daily Sales")]
        public List<AdminDailySalesViewModel> DailySales { get; set; } = new List<AdminDailySalesViewModel>();

        [Display(Name = "Top Organizers")]
        public List<TopOrganizerSalesViewModel> TopOrganizers { get; set; } = new List<TopOrganizerSalesViewModel>();

        [Display(Name = "Category Performance")]
        public List<AdminCategoryPerformanceViewModel> CategoryPerformance { get; set; } = new List<AdminCategoryPerformanceViewModel>();

        [Display(Name = "Monthly Trends")]
        public List<AdminMonthlySalesViewModel> MonthlyTrends { get; set; } = new List<AdminMonthlySalesViewModel>();
    }

    public class AdminDailySalesViewModel
    {
        public DateTime Date { get; set; }
        public string FormattedDate => Date.ToString("MMM dd");
        public int TicketsSold { get; set; }
        public decimal Revenue { get; set; }
        public string FormattedRevenue => Revenue.ToString("C");
        public int EventsCount { get; set; }
    }

    public class TopOrganizerSalesViewModel
    {
        public string OrganizerId { get; set; } = string.Empty;
        public string OrganizerName { get; set; } = string.Empty;
        public int EventsCount { get; set; }
        public int TicketsSold { get; set; }
        public decimal Revenue { get; set; }
        public string FormattedRevenue => Revenue.ToString("C");
        public decimal AverageRevenuePerEvent => EventsCount > 0 ? Revenue / EventsCount : 0;
        public string FormattedAverageRevenuePerEvent => AverageRevenuePerEvent.ToString("C");
    }

    public class AdminCategoryPerformanceViewModel
    {
        public string Category { get; set; } = string.Empty;
        public int EventsCount { get; set; }
        public int TicketsSold { get; set; }
        public decimal Revenue { get; set; }
        public string FormattedRevenue => Revenue.ToString("C");
        public decimal AverageRevenuePerEvent => EventsCount > 0 ? Revenue / EventsCount : 0;
        public string FormattedAverageRevenuePerEvent => AverageRevenuePerEvent.ToString("C");
        public decimal MarketShare { get; set; }
        public string FormattedMarketShare => $"{MarketShare:F1}%";
    }

    public class AdminMonthlySalesViewModel
    {
        public string Month { get; set; } = string.Empty;
        public int TicketsSold { get; set; }
        public decimal Revenue { get; set; }
        public string FormattedRevenue => Revenue.ToString("C");
        public int EventsCount { get; set; }
        public int OrganizersCount { get; set; }
    }
}