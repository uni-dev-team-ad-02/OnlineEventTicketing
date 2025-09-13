using System.ComponentModel.DataAnnotations;

namespace OnlineEventTicketing.Models.ViewModels
{
    public class UserDisplayViewModel
    {
        public string Id { get; set; } = string.Empty;
        
        [Display(Name = "Email")]
        public string? Email { get; set; }
        
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;
        
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;
        
        [Display(Name = "Full Name")]
        public string FullName => $"{FirstName} {LastName}";
        
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }
        
        [Display(Name = "Email Confirmed")]
        public bool EmailConfirmed { get; set; }
        
        [Display(Name = "Lockout End")]
        public DateTimeOffset? LockoutEnd { get; set; }
        
        [Display(Name = "Status")]
        public bool IsActive { get; set; }
        
        [Display(Name = "Status")]
        public string StatusDisplayName => IsActive ? "Active" : "Inactive";
        
        [Display(Name = "Status")]
        public string StatusBadgeClass => IsActive ? "bg-success" : "bg-danger";
        
        [Display(Name = "Loyalty Points")]
        public int LoyaltyPoints { get; set; }
        
        [Display(Name = "Roles")]
        public List<string> Roles { get; set; } = new List<string>();
        
        [Display(Name = "Roles")]
        public string RolesDisplayText => string.Join(", ", Roles);
        
        [Display(Name = "Registration Date")]
        public DateTime RegistrationDate { get; set; }
        
        [Display(Name = "Registration Date")]
        public string FormattedRegistrationDate => RegistrationDate.ToString("MMM dd, yyyy");
        
        [Display(Name = "Total Tickets")]
        public int TotalTicketsPurchased { get; set; }
        
        [Display(Name = "Total Spent")]
        public decimal TotalAmountSpent { get; set; }
        
        [Display(Name = "Total Spent")]
        public string FormattedTotalAmountSpent => TotalAmountSpent.ToString("C");
        
        public string PrimaryRoleBadgeClass => Roles.FirstOrDefault() switch
        {
            "Admin" => "bg-danger",
            "EventOrganizer" => "bg-primary",
            "Customer" => "bg-success",
            _ => "bg-secondary"
        };
    }
}