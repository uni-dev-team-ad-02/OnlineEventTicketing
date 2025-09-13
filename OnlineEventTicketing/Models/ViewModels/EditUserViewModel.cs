using System.ComponentModel.DataAnnotations;

namespace OnlineEventTicketing.Models.ViewModels
{
    public class EditUserViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Required]
        [Display(Name = "First Name")]
        [StringLength(50, ErrorMessage = "First name cannot be longer than 50 characters.")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Last Name")]
        [StringLength(50, ErrorMessage = "Last name cannot be longer than 50 characters.")]
        public string LastName { get; set; } = string.Empty;

        [Phone]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Loyalty Points")]
        [Range(0, int.MaxValue, ErrorMessage = "Loyalty points must be non-negative.")]
        public int LoyaltyPoints { get; set; }

        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Display(Name = "Current Roles")]
        public List<string> CurrentRoles { get; set; } = new List<string>();

        [Display(Name = "Available Roles")]
        public List<string> AvailableRoles { get; set; } = new List<string>();

        [Display(Name = "Selected Roles")]
        public List<string> SelectedRoles { get; set; } = new List<string>();
    }
}