using OnlineEventTicketing.Helpers;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineEventTicketing.Data.Entity
{
    public class Event : CommonProps
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime Date { get; set; }

        [Required]
        [StringLength(200)]
        public string Location { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Category { get; set; } = string.Empty;

        [Required]
        public int Capacity { get; set; }

        public int AvailableTickets { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal BasePrice { get; set; }

        [StringLength(500)]
        public string ImageUrl { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        [Required]
        public string OrganizerId { get; set; } = string.Empty;

        [ForeignKey("OrganizerId")]
        public virtual ApplicationUser Organizer { get; set; } = null!;

        public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
        public virtual ICollection<Promotion> Promotions { get; set; } = new List<Promotion>();
    }
}