using System.ComponentModel.DataAnnotations;

namespace OnlineEventTicketing.Models.ViewModels
{
    public class OrganizerRevenueReportViewModel
    {
        [Display(Name = "Total Revenue")]
        public decimal TotalRevenue { get; set; }

        [Display(Name = "Total Revenue")]
        public string FormattedTotalRevenue => TotalRevenue.ToString("C");

        [Display(Name = "Total Tickets Sold")]
        public int TotalTicketsSold { get; set; }

        [Display(Name = "Total Events")]
        public int TotalEvents { get; set; }

        [Display(Name = "Active Events")]
        public int ActiveEvents { get; set; }

        [Display(Name = "Average Revenue per Event")]
        public decimal AverageRevenuePerEvent => TotalEvents > 0 ? TotalRevenue / TotalEvents : 0;

        [Display(Name = "Average Revenue per Event")]
        public string FormattedAverageRevenuePerEvent => AverageRevenuePerEvent.ToString("C");

        [Display(Name = "Average Ticket Price")]
        public decimal AverageTicketPrice => TotalTicketsSold > 0 ? TotalRevenue / TotalTicketsSold : 0;

        [Display(Name = "Average Ticket Price")]
        public string FormattedAverageTicketPrice => AverageTicketPrice.ToString("C");

        [Display(Name = "Report Period")]
        public DateTime StartDate { get; set; }

        [Display(Name = "Report Period")]
        public DateTime EndDate { get; set; }

        [Display(Name = "Report Period")]
        public string FormattedReportPeriod => $"{StartDate:MMM dd, yyyy} - {EndDate:MMM dd, yyyy}";

        [Display(Name = "Monthly Breakdown")]
        public List<MonthlyRevenueViewModel> MonthlyBreakdown { get; set; } = new List<MonthlyRevenueViewModel>();

        [Display(Name = "Event Revenue Breakdown")]
        public List<EventRevenueViewModel> EventBreakdown { get; set; } = new List<EventRevenueViewModel>();

        [Display(Name = "Category Performance")]
        public List<CategoryPerformanceViewModel> CategoryPerformance { get; set; } = new List<CategoryPerformanceViewModel>();
    }

    public class MonthlyRevenueViewModel
    {
        public string Month { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public string FormattedRevenue => Revenue.ToString("C");
        public int TicketsSold { get; set; }
        public int EventsCount { get; set; }
    }

    public class EventRevenueViewModel
    {
        public int EventId { get; set; }
        public string EventTitle { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string FormattedEventDate => EventDate.ToString("MMM dd, yyyy");
        public decimal Revenue { get; set; }
        public string FormattedRevenue => Revenue.ToString("C");
        public int TicketsSold { get; set; }
        public int Capacity { get; set; }
        public decimal SellThroughRate => Capacity > 0 ? (decimal)TicketsSold / Capacity * 100 : 0;
        public string FormattedSellThroughRate => $"{SellThroughRate:F1}%";
    }

    public class CategoryPerformanceViewModel
    {
        public string Category { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public string FormattedRevenue => Revenue.ToString("C");
        public int TicketsSold { get; set; }
        public int EventsCount { get; set; }
        public decimal AverageRevenuePerEvent => EventsCount > 0 ? Revenue / EventsCount : 0;
        public string FormattedAverageRevenuePerEvent => AverageRevenuePerEvent.ToString("C");
    }
}