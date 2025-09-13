using System.ComponentModel.DataAnnotations;

namespace OnlineEventTicketing.Models.ViewModels
{
    public class UserStatsViewModel
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

        [Display(Name = "Active Percentage")]
        public double ActivePercentage => TotalUsers > 0 ? Math.Round((double)ActiveUsers / TotalUsers * 100, 1) : 0;
    }
}