using System.ComponentModel.DataAnnotations;

namespace OnlineEventTicketing.Models.ViewModels
{
    public class CreatePromotionViewModel
    {
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
        public DateTime StartDate { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "End Date")]
        [DataType(DataType.DateTime)]
        public DateTime EndDate { get; set; } = DateTime.Now.AddDays(30);

        [Required]
        [Display(Name = "Event")]
        public int EventId { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        // For dropdown population
        public List<EventDisplayViewModel> AvailableEvents { get; set; } = new List<EventDisplayViewModel>();
    }
}