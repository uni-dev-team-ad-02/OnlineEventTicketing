using OnlineEventTicketing.Data.Entity;

namespace OnlineEventTicketing.Models.ViewModels
{
    public class TicketDisplayViewModel
    {
        public int Id { get; set; }
        public string QrCode { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string SeatNumber { get; set; } = string.Empty;
        public TicketStatus Status { get; set; }
        public DateTime PurchaseDate { get; set; }

        public int EventId { get; set; }
        public string EventTitle { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string EventLocation { get; set; } = string.Empty;
        public string EventCategory { get; set; } = string.Empty;

        public string CustomerId { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;

        public string StatusDisplayName => Status switch
        {
            TicketStatus.Active => "Active",
            TicketStatus.Used => "Used",
            TicketStatus.Cancelled => "Cancelled",
            TicketStatus.Refunded => "Refunded",
            _ => "Unknown"
        };

        public string StatusClass => Status switch
        {
            TicketStatus.Active => "bg-success",
            TicketStatus.Used => "bg-info",
            TicketStatus.Cancelled => "bg-warning",
            TicketStatus.Refunded => "bg-danger",
            _ => "bg-secondary"
        };

        public string FormattedPrice => Price.ToString("C");
        public string FormattedEventDate => EventDate.ToString("MMM dd, yyyy 'at' h:mm tt");
        public string FormattedPurchaseDate => PurchaseDate.ToString("MMM dd, yyyy 'at' h:mm tt");
        
        public bool CanBeCancelled => Status == TicketStatus.Active && EventDate > DateTime.UtcNow.AddHours(24);
        public bool CanBeRefunded => Status == TicketStatus.Active && EventDate > DateTime.UtcNow.AddDays(7);
        public bool IsUpcoming => EventDate > DateTime.UtcNow;
    }
}