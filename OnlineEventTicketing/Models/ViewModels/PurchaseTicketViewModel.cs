using System.ComponentModel.DataAnnotations;

namespace OnlineEventTicketing.Models.ViewModels
{
    public class PurchaseTicketViewModel
    {
        public int EventId { get; set; }
        public string EventTitle { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string EventLocation { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public int AvailableTickets { get; set; }

        [Range(1, 10, ErrorMessage = "You can purchase between 1 and 10 tickets")]
        [Display(Name = "Number of Tickets")]
        public int TicketQuantity { get; set; } = 1;

        [Display(Name = "Promotion Code")]
        public string? PromotionCode { get; set; }

        [Required(ErrorMessage = "Payment method is required")]
        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; } = "CreditCard";

        public decimal TotalPrice { get; set; }
        public decimal Discount { get; set; }
        public bool IsPromotionValid { get; set; }

        public string FormattedBasePrice => BasePrice.ToString("C");
        public string FormattedTotalPrice => TotalPrice.ToString("C");
        public string FormattedDiscount => Discount.ToString("C");
        public string FormattedEventDate => EventDate.ToString("MMM dd, yyyy 'at' h:mm tt");

        public List<string> PaymentMethods { get; set; } = new List<string>
        {
            "CreditCard",
            "DebitCard",
            "PayPal",
            "BankTransfer"
        };
    }
}