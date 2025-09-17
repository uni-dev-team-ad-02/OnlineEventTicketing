using System.ComponentModel.DataAnnotations;

namespace OnlineEventTicketing.Models.ViewModels
{
    public class EventSalesReportViewModel
    {
        public int EventId { get; set; }

        [Display(Name = "Event Title")]
        public string EventTitle { get; set; } = string.Empty;

        [Display(Name = "Event Date")]
        public DateTime EventDate { get; set; }

        [Display(Name = "Event Date")]
        public string FormattedEventDate => EventDate.ToString("MMM dd, yyyy");

        [Display(Name = "Location")]
        public string Location { get; set; } = string.Empty;

        [Display(Name = "Category")]
        public string Category { get; set; } = string.Empty;

        [Display(Name = "Capacity")]
        public int Capacity { get; set; }

        [Display(Name = "Tickets Sold")]
        public int TicketsSold { get; set; }

        [Display(Name = "Revenue")]
        public decimal Revenue { get; set; }

        [Display(Name = "Revenue")]
        public string FormattedRevenue => Revenue.ToString("C");

        [Display(Name = "Sell-through Rate")]
        public decimal SellThroughRate => Capacity > 0 ? (decimal)TicketsSold / Capacity * 100 : 0;

        [Display(Name = "Sell-through Rate")]
        public string FormattedSellThroughRate => $"{SellThroughRate:F1}%";

        [Display(Name = "Status")]
        public bool IsActive { get; set; }

        [Display(Name = "Status")]
        public string StatusDisplayName => IsActive ? "Active" : "Inactive";

        [Display(Name = "Status")]
        public string StatusBadgeClass => IsActive ? "bg-success" : "bg-secondary";

        [Display(Name = "Performance")]
        public string PerformanceRating => SellThroughRate switch
        {
            >= 90 => "Excellent",
            >= 75 => "Good",
            >= 50 => "Average",
            >= 25 => "Below Average",
            _ => "Poor"
        };

        [Display(Name = "Performance")]
        public string PerformanceBadgeClass => SellThroughRate switch
        {
            >= 90 => "bg-success",
            >= 75 => "bg-primary",
            >= 50 => "bg-info",
            >= 25 => "bg-warning",
            _ => "bg-danger"
        };
    }
}