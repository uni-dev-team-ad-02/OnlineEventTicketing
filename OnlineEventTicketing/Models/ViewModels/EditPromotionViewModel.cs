using System.ComponentModel.DataAnnotations;

namespace OnlineEventTicketing.Models.ViewModels
{
    public class EditPromotionViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "Code cannot be longer than 50 characters.")]
        [Display(Name = "Promotion Code")]
        [RegularExpression(@"^[A-Z0-9]+$", ErrorMessage = "Code must contain only uppercase letters and numbers.")]
        public string Code { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot be longer than 500 characters.")]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(0.01, 100, ErrorMessage = "Discount percentage must be between 0.01% and 100%.")]
        [Display(Name = "Discount Percentage")]
        public decimal DiscountPercentage { get; set; }

        [Required]
        [Display(Name = "Start Date")]
        [DataType(DataType.DateTime)]
        public DateTime StartDate { get; set; }

        [Required]
        [Display(Name = "End Date")]
        [DataType(DataType.DateTime)]
        public DateTime EndDate { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; }

        [Display(Name = "Event")]
        public int EventId { get; set; }

        [Display(Name = "Event Title")]
        public string EventTitle { get; set; } = string.Empty;

        [Display(Name = "Event Date")]
        public DateTime EventDate { get; set; }

        [Display(Name = "Event Location")]
        public string EventLocation { get; set; } = string.Empty;
    }
}