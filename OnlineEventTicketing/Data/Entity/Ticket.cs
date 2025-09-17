using OnlineEventTicketing.Helpers;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineEventTicketing.Data.Entity
{
    public enum TicketStatus
    {
        Active,
        Used,
        Cancelled,
        Refunded
    }

    public class Ticket : CommonProps
    {
        [Required]
        [StringLength(500)]
        public string QrCode { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        [StringLength(20)]
        public string SeatNumber { get; set; } = string.Empty;

        [Required]
        public TicketStatus Status { get; set; } = TicketStatus.Active;

        [Required]
        public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;

        [Required]
        public int EventId { get; set; }

        [Required]
        public string CustomerId { get; set; } = string.Empty;

        [ForeignKey("EventId")]
        public virtual Event Event { get; set; } = null!;

        [ForeignKey("CustomerId")]
        public virtual ApplicationUser Customer { get; set; } = null!;

        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}