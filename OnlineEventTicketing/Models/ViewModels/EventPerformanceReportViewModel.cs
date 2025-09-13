using System.ComponentModel.DataAnnotations;

namespace OnlineEventTicketing.Models.ViewModels
{
    public class EventPerformanceReportViewModel
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

        [Display(Name = "Base Price")]
        public decimal BasePrice { get; set; }

        [Display(Name = "Base Price")]
        public string FormattedBasePrice => BasePrice.ToString("C");

        [Display(Name = "Capacity")]
        public int Capacity { get; set; }

        [Display(Name = "Tickets Sold")]
        public int TicketsSold { get; set; }

        [Display(Name = "Available Tickets")]
        public int AvailableTickets { get; set; }

        [Display(Name = "Total Revenue")]
        public decimal TotalRevenue { get; set; }

        [Display(Name = "Total Revenue")]
        public string FormattedTotalRevenue => TotalRevenue.ToString("C");

        [Display(Name = "Sell-through Rate")]
        public decimal SellThroughRate => Capacity > 0 ? (decimal)TicketsSold / Capacity * 100 : 0;

        [Display(Name = "Sell-through Rate")]
        public string FormattedSellThroughRate => $"{SellThroughRate:F1}%";

        [Display(Name = "Average Ticket Price")]
        public decimal AverageTicketPrice => TicketsSold > 0 ? TotalRevenue / TicketsSold : 0;

        [Display(Name = "Average Ticket Price")]
        public string FormattedAverageTicketPrice => AverageTicketPrice.ToString("C");

        [Display(Name = "Active Tickets")]
        public int ActiveTickets { get; set; }

        [Display(Name = "Cancelled Tickets")]
        public int CancelledTickets { get; set; }

        [Display(Name = "Refunded Tickets")]
        public int RefundedTickets { get; set; }

        [Display(Name = "Cancellation Rate")]
        public decimal CancellationRate => TicketsSold > 0 ? (decimal)CancelledTickets / TicketsSold * 100 : 0;

        [Display(Name = "Cancellation Rate")]
        public string FormattedCancellationRate => $"{CancellationRate:F1}%";

        [Display(Name = "Performance Rating")]
        public string PerformanceRating => GetPerformanceRating();

        [Display(Name = "Performance Rating")]
        public string PerformanceBadgeClass => GetPerformanceBadgeClass();

        [Display(Name = "Sales Timeline")]
        public List<SalesTimelineViewModel> SalesTimeline { get; set; } = new List<SalesTimelineViewModel>();

        [Display(Name = "Promotions Used")]
        public List<PromotionUsageViewModel> PromotionsUsed { get; set; } = new List<PromotionUsageViewModel>();

        [Display(Name = "Days Until Event")]
        public int DaysUntilEvent => (int)(EventDate - DateTime.Now).TotalDays;

        [Display(Name = "Days Until Event")]
        public string FormattedDaysUntilEvent
        {
            get
            {
                var days = DaysUntilEvent;
                if (days < 0)
                    return "Event Completed";
                if (days == 0)
                    return "Today";
                if (days == 1)
                    return "Tomorrow";
                return $"{days} days";
            }
        }

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

        private string GetPerformanceBadgeClass()
        {
            return SellThroughRate switch
            {
                >= 90 => "bg-success",
                >= 75 => "bg-primary",
                >= 50 => "bg-info",
                >= 25 => "bg-warning",
                _ => "bg-danger"
            };
        }
    }

    public class SalesTimelineViewModel
    {
        public DateTime Date { get; set; }
        public string FormattedDate => Date.ToString("MMM dd");
        public int TicketsSold { get; set; }
        public decimal Revenue { get; set; }
        public string FormattedRevenue => Revenue.ToString("C");
    }

    public class PromotionUsageViewModel
    {
        public string PromotionCode { get; set; } = string.Empty;
        public decimal DiscountPercentage { get; set; }
        public string FormattedDiscountPercentage => $"{DiscountPercentage}%";
        public int UsageCount { get; set; }
        public decimal TotalDiscountGiven { get; set; }
        public string FormattedTotalDiscountGiven => TotalDiscountGiven.ToString("C");
    }
}