using System.ComponentModel.DataAnnotations;

namespace OnlineEventTicketing.Models.ViewModels
{
    public class PromotionDisplayViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Promotion Code")]
        public string Code { get; set; } = string.Empty;

        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Discount Percentage")]
        public decimal DiscountPercentage { get; set; }

        [Display(Name = "Discount")]
        public string FormattedDiscountPercentage => $"{DiscountPercentage}%";

        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }

        [Display(Name = "Start Date")]
        public string FormattedStartDate => StartDate.ToString("MMM dd, yyyy");

        [Display(Name = "End Date")]
        public string FormattedEndDate => EndDate.ToString("MMM dd, yyyy");

        [Display(Name = "Duration")]
        public string FormattedDuration => $"{FormattedStartDate} - {FormattedEndDate}";

        [Display(Name = "Status")]
        public bool IsActive { get; set; }

        [Display(Name = "Event ID")]
        public int EventId { get; set; }

        [Display(Name = "Event Title")]
        public string EventTitle { get; set; } = string.Empty;

        [Display(Name = "Event Date")]
        public DateTime EventDate { get; set; }

        [Display(Name = "Event Date")]
        public string FormattedEventDate => EventDate.ToString("MMM dd, yyyy");

        [Display(Name = "Event Location")]
        public string EventLocation { get; set; } = string.Empty;

        [Display(Name = "Created Date")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Created Date")]
        public string FormattedCreatedAt => CreatedAt.ToString("MMM dd, yyyy");

        [Display(Name = "Status")]
        public string StatusDisplayName => GetStatusDisplayName();

        [Display(Name = "Status")]
        public string StatusBadgeClass => GetStatusBadgeClass();

        [Display(Name = "Status")]
        public bool IsCurrentlyActive => GetIsCurrentlyActive();

        private string GetStatusDisplayName()
        {
            if (!IsActive)
                return "Disabled";

            var now = DateTime.Now;
            
            if (now < StartDate)
                return "Scheduled";
            
            if (now > EndDate)
                return "Expired";
                
            return "Active";
        }

        private string GetStatusBadgeClass()
        {
            if (!IsActive)
                return "bg-secondary";

            var now = DateTime.Now;
            
            if (now < StartDate)
                return "bg-warning";
            
            if (now > EndDate)
                return "bg-danger";
                
            return "bg-success";
        }

        private bool GetIsCurrentlyActive()
        {
            if (!IsActive)
                return false;

            var now = DateTime.Now;
            return now >= StartDate && now <= EndDate;
        }

        [Display(Name = "Days Remaining")]
        public int DaysRemaining
        {
            get
            {
                if (!IsActive || DateTime.Now > EndDate)
                    return 0;
                
                if (DateTime.Now < StartDate)
                    return (int)(StartDate - DateTime.Now).TotalDays;
                    
                return (int)(EndDate - DateTime.Now).TotalDays;
            }
        }

        [Display(Name = "Days Remaining")]
        public string FormattedDaysRemaining
        {
            get
            {
                var days = DaysRemaining;
                if (days == 0)
                    return "Expired/Inactive";
                
                if (DateTime.Now < StartDate)
                    return $"Starts in {days} days";
                    
                return $"{days} days left";
            }
        }
    }
}