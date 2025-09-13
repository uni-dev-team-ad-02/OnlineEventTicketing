using System.ComponentModel.DataAnnotations;

namespace OnlineEventTicketing.Models.ViewModels
{
    public class AdminUsersReportViewModel
    {
        [Display(Name = "Total Users")]
        public int TotalUsers { get; set; }

        [Display(Name = "Active Users")]
        public int ActiveUsers { get; set; }

        [Display(Name = "Inactive Users")]
        public int InactiveUsers { get; set; }

        [Display(Name = "Total Customers")]
        public int TotalCustomers { get; set; }

        [Display(Name = "Total Organizers")]
        public int TotalOrganizers { get; set; }

        [Display(Name = "Total Admins")]
        public int TotalAdmins { get; set; }

        [Display(Name = "New Users This Month")]
        public int NewUsersThisMonth { get; set; }

        [Display(Name = "Active Users Percentage")]
        public decimal ActiveUsersPercentage => TotalUsers > 0 ? (decimal)ActiveUsers / TotalUsers * 100 : 0;

        [Display(Name = "Active Users Percentage")]
        public string FormattedActiveUsersPercentage => $"{ActiveUsersPercentage:F1}%";

        [Display(Name = "Report Period")]
        public DateTime StartDate { get; set; }

        [Display(Name = "Report Period")]
        public DateTime EndDate { get; set; }

        [Display(Name = "Report Period")]
        public string FormattedReportPeriod => $"{StartDate:MMM dd, yyyy} - {EndDate:MMM dd, yyyy}";

        [Display(Name = "User Registration Trends")]
        public List<UserRegistrationTrendViewModel> RegistrationTrends { get; set; } = new List<UserRegistrationTrendViewModel>();

        [Display(Name = "Role Distribution")]
        public List<RoleDistributionViewModel> RoleDistribution { get; set; } = new List<RoleDistributionViewModel>();

        [Display(Name = "Top Customers")]
        public List<TopCustomerViewModel> TopCustomers { get; set; } = new List<TopCustomerViewModel>();

        [Display(Name = "Top Organizers")]
        public List<TopOrganizerViewModel> TopOrganizers { get; set; } = new List<TopOrganizerViewModel>();

        [Display(Name = "Recent Users")]
        public List<RecentUserViewModel> RecentUsers { get; set; } = new List<RecentUserViewModel>();
    }

    public class UserRegistrationTrendViewModel
    {
        public string Period { get; set; } = string.Empty;
        public int NewUsers { get; set; }
        public int TotalUsers { get; set; }
        public decimal GrowthRate { get; set; }
        public string FormattedGrowthRate => $"{GrowthRate:F1}%";
    }

    public class RoleDistributionViewModel
    {
        public string Role { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal Percentage { get; set; }
        public string FormattedPercentage => $"{Percentage:F1}%";
        public string BadgeClass => Role switch
        {
            "Admin" => "bg-danger",
            "EventOrganizer" => "bg-primary",
            "Customer" => "bg-success",
            _ => "bg-secondary"
        };
    }

    public class TopCustomerViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int TicketsPurchased { get; set; }
        public decimal TotalSpent { get; set; }
        public string FormattedTotalSpent => TotalSpent.ToString("C");
        public int LoyaltyPoints { get; set; }
        public DateTime RegistrationDate { get; set; }
        public string FormattedRegistrationDate => RegistrationDate.ToString("MMM dd, yyyy");
    }

    public class TopOrganizerViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int EventsCreated { get; set; }
        public int TicketsSold { get; set; }
        public decimal TotalRevenue { get; set; }
        public string FormattedTotalRevenue => TotalRevenue.ToString("C");
        public DateTime RegistrationDate { get; set; }
        public string FormattedRegistrationDate => RegistrationDate.ToString("MMM dd, yyyy");
    }

    public class RecentUserViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime RegistrationDate { get; set; }
        public string FormattedRegistrationDate => RegistrationDate.ToString("MMM dd, yyyy");
        public bool IsActive { get; set; }
        public string StatusBadgeClass => IsActive ? "bg-success" : "bg-danger";
        public string StatusDisplayName => IsActive ? "Active" : "Inactive";
        public string RoleBadgeClass => Role switch
        {
            "Admin" => "bg-danger",
            "EventOrganizer" => "bg-primary",
            "Customer" => "bg-success",
            _ => "bg-secondary"
        };
    }
}