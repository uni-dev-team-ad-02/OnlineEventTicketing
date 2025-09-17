using System.ComponentModel.DataAnnotations;

namespace OnlineEventTicketing.Models.ViewModels
{
    public class OrganizerSalesReportViewModel
    {
        [Display(Name = "Total Tickets Sold")]
        public int TotalTicketsSold { get; set; }

        [Display(Name = "Total Revenue")]
        public decimal TotalRevenue { get; set; }

        [Display(Name = "Total Revenue")]
        public string FormattedTotalRevenue => TotalRevenue.ToString("C");

        [Display(Name = "Total Events")]
        public int TotalEvents { get; set; }

        [Display(Name = "Active Tickets")]
        public int ActiveTickets { get; set; }

        [Display(Name = "Cancelled Tickets")]
        public int CancelledTickets { get; set; }

        [Display(Name = "Refunded Tickets")]
        public int RefundedTickets { get; set; }

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
        public List<DailySalesViewModel> DailySales { get; set; } = new List<DailySalesViewModel>();

        [Display(Name = "Event Sales Performance")]
        public List<EventSalesPerformanceViewModel> EventSalesPerformance { get; set; } = new List<EventSalesPerformanceViewModel>();

        [Display(Name = "Peak Sales Days")]
        public List<PeakSalesDayViewModel> PeakSalesDays { get; set; } = new List<PeakSalesDayViewModel>();
    }

    public class DailySalesViewModel
    {
        public DateTime Date { get; set; }
        public string FormattedDate => Date.ToString("MMM dd");
        public int TicketsSold { get; set; }
        public decimal Revenue { get; set; }
        public string FormattedRevenue => Revenue.ToString("C");
    }

    public class EventSalesPerformanceViewModel
    {
        public int EventId { get; set; }
        public string EventTitle { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string FormattedEventDate => EventDate.ToString("MMM dd, yyyy");
        public int TicketsSold { get; set; }
        public int Capacity { get; set; }
        public decimal Revenue { get; set; }
        public string FormattedRevenue => Revenue.ToString("C");
        public decimal SellThroughRate => Capacity > 0 ? (decimal)TicketsSold / Capacity * 100 : 0;
        public string FormattedSellThroughRate => $"{SellThroughRate:F1}%";
        public string PerformanceRating => GetPerformanceRating();

        private string GetPerformanceRating()
        {
            return SellThroughRate switch
            {
                >= 90 => "Excellent",
                >= 75 => "Good",
                >= 50 => "Average",
                >= 25 => "Below Average",
                _ => "Poor"
            };
        }

        public string PerformanceBadgeClass => SellThroughRate switch
        {
            >= 90 => "bg-success",
            >= 75 => "bg-primary",
            >= 50 => "bg-info",
            >= 25 => "bg-warning",
            _ => "bg-danger"
        };
    }

    public class PeakSalesDayViewModel
    {
        public DateTime Date { get; set; }
        public string FormattedDate => Date.ToString("MMM dd, yyyy");
        public int TicketsSold { get; set; }
        public decimal Revenue { get; set; }
        public string FormattedRevenue => Revenue.ToString("C");
        public string Reason { get; set; } = string.Empty;
    }
}