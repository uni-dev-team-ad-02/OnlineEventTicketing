using System.ComponentModel.DataAnnotations;

namespace OnlineEventTicketing.Models.ViewModels
{
    public class AdminEventsReportViewModel
    {
        [Display(Name = "Total Events")]
        public int TotalEvents { get; set; }

        [Display(Name = "Active Events")]
        public int ActiveEvents { get; set; }

        [Display(Name = "Inactive Events")]
        public int InactiveEvents { get; set; }

        [Display(Name = "Upcoming Events")]
        public int UpcomingEvents { get; set; }

        [Display(Name = "Past Events")]
        public int PastEvents { get; set; }

        [Display(Name = "Total Capacity")]
        public int TotalCapacity { get; set; }

        [Display(Name = "Total Tickets Sold")]
        public int TotalTicketsSold { get; set; }

        [Display(Name = "Overall Sell-through Rate")]
        public decimal OverallSellThroughRate => TotalCapacity > 0 ? (decimal)TotalTicketsSold / TotalCapacity * 100 : 0;

        [Display(Name = "Overall Sell-through Rate")]
        public string FormattedOverallSellThroughRate => $"{OverallSellThroughRate:F1}%";

        [Display(Name = "Average Event Capacity")]
        public decimal AverageEventCapacity => TotalEvents > 0 ? (decimal)TotalCapacity / TotalEvents : 0;

        [Display(Name = "Average Event Capacity")]
        public string FormattedAverageEventCapacity => $"{AverageEventCapacity:F0}";

        [Display(Name = "Report Period")]
        public DateTime StartDate { get; set; }

        [Display(Name = "Report Period")]
        public DateTime EndDate { get; set; }

        [Display(Name = "Report Period")]
        public string FormattedReportPeriod => $"{StartDate:MMM dd, yyyy} - {EndDate:MMM dd, yyyy}";

        [Display(Name = "Top Performing Events")]
        public List<TopPerformingEventViewModel> TopPerformingEvents { get; set; } = new List<TopPerformingEventViewModel>();

        [Display(Name = "Category Analysis")]
        public List<EventCategoryAnalysisViewModel> CategoryAnalysis { get; set; } = new List<EventCategoryAnalysisViewModel>();

        [Display(Name = "Monthly Event Creation")]
        public List<MonthlyEventCreationViewModel> MonthlyEventCreation { get; set; } = new List<MonthlyEventCreationViewModel>();

        [Display(Name = "Organizer Performance")]
        public List<OrganizerPerformanceViewModel> OrganizerPerformance { get; set; } = new List<OrganizerPerformanceViewModel>();

        [Display(Name = "Recent Events")]
        public List<RecentEventViewModel> RecentEvents { get; set; } = new List<RecentEventViewModel>();
    }

    public class TopPerformingEventViewModel
    {
        public int EventId { get; set; }
        public string EventTitle { get; set; } = string.Empty;
        public string OrganizerName { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string FormattedEventDate => EventDate.ToString("MMM dd, yyyy");
        public string Location { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public int TicketsSold { get; set; }
        public decimal Revenue { get; set; }
        public string FormattedRevenue => Revenue.ToString("C");
        public decimal SellThroughRate => Capacity > 0 ? (decimal)TicketsSold / Capacity * 100 : 0;
        public string FormattedSellThroughRate => $"{SellThroughRate:F1}%";
        public string PerformanceRating => SellThroughRate switch
        {
            >= 90 => "Excellent",
            >= 75 => "Good",
            >= 50 => "Average",
            >= 25 => "Below Average",
            _ => "Poor"
        };
        public string PerformanceBadgeClass => SellThroughRate switch
        {
            >= 90 => "bg-success",
            >= 75 => "bg-primary",
            >= 50 => "bg-info",
            >= 25 => "bg-warning",
            _ => "bg-danger"
        };
    }

    public class EventCategoryAnalysisViewModel
    {
        public string Category { get; set; } = string.Empty;
        public int EventCount { get; set; }
        public int TotalCapacity { get; set; }
        public int TicketsSold { get; set; }
        public decimal Revenue { get; set; }
        public string FormattedRevenue => Revenue.ToString("C");
        public decimal SellThroughRate => TotalCapacity > 0 ? (decimal)TicketsSold / TotalCapacity * 100 : 0;
        public string FormattedSellThroughRate => $"{SellThroughRate:F1}%";
        public decimal AverageRevenuePerEvent => EventCount > 0 ? Revenue / EventCount : 0;
        public string FormattedAverageRevenuePerEvent => AverageRevenuePerEvent.ToString("C");
        public decimal MarketShare { get; set; }
        public string FormattedMarketShare => $"{MarketShare:F1}%";
    }

    public class MonthlyEventCreationViewModel
    {
        public string Month { get; set; } = string.Empty;
        public int EventsCreated { get; set; }
        public int ActiveOrganizers { get; set; }
        public decimal AverageCapacity { get; set; }
        public string FormattedAverageCapacity => $"{AverageCapacity:F0}";
    }

    public class OrganizerPerformanceViewModel
    {
        public string OrganizerId { get; set; } = string.Empty;
        public string OrganizerName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int EventsCreated { get; set; }
        public int ActiveEvents { get; set; }
        public int TotalCapacity { get; set; }
        public int TicketsSold { get; set; }
        public decimal Revenue { get; set; }
        public string FormattedRevenue => Revenue.ToString("C");
        public decimal SellThroughRate => TotalCapacity > 0 ? (decimal)TicketsSold / TotalCapacity * 100 : 0;
        public string FormattedSellThroughRate => $"{SellThroughRate:F1}%";
        public string PerformanceRating => SellThroughRate switch
        {
            >= 80 => "Excellent",
            >= 60 => "Good",
            >= 40 => "Average",
            >= 20 => "Below Average",
            _ => "Poor"
        };
        public string PerformanceBadgeClass => SellThroughRate switch
        {
            >= 80 => "bg-success",
            >= 60 => "bg-primary",
            >= 40 => "bg-info",
            >= 20 => "bg-warning",
            _ => "bg-danger"
        };
    }

    public class RecentEventViewModel
    {
        public int EventId { get; set; }
        public string EventTitle { get; set; } = string.Empty;
        public string OrganizerName { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string FormattedEventDate => EventDate.ToString("MMM dd, yyyy");
        public string Category { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string StatusBadgeClass => IsActive ? "bg-success" : "bg-secondary";
        public string StatusDisplayName => IsActive ? "Active" : "Inactive";
        public int TicketsSold { get; set; }
        public int Capacity { get; set; }
        public DateTime CreatedAt { get; set; }
        public string FormattedCreatedAt => CreatedAt.ToString("MMM dd, yyyy");
    }
}