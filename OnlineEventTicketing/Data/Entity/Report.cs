using OnlineEventTicketing.Helpers;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineEventTicketing.Data.Entity
{
    public enum ReportType
    {
        SalesReport,
        RevenueReport,
        UserReport,
        EventReport,
        PaymentReport
    }

    public class Report : CommonProps
    {
        [Required]
        public ReportType Type { get; set; }

        [Required]
        public DateTime GeneratedDate { get; set; } = DateTime.UtcNow;

        [Required]
        public string OrganizerId { get; set; } = string.Empty;

        [StringLength(2000)]
        public string Content { get; set; } = string.Empty;

        [ForeignKey("OrganizerId")]
        public virtual ApplicationUser Organizer { get; set; } = null!;
    }
}