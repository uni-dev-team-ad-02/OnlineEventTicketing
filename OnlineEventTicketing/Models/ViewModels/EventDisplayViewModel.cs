using OnlineEventTicketing.Data.Entity;

namespace OnlineEventTicketing.Models.ViewModels
{
    public class EventDisplayViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Location { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public int AvailableTickets { get; set; }
        public decimal BasePrice { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string OrganizerId { get; set; } = string.Empty;
        public string OrganizerName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        
        public bool IsUpcoming => Date > DateTime.UtcNow;
        public bool HasAvailableTickets => AvailableTickets > 0;
        public string FormattedDate => Date.ToString("MMM dd, yyyy 'at' h:mm tt");
        public string FormattedPrice => BasePrice.ToString("C");
    }
}